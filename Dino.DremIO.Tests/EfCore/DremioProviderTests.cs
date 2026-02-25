using Dino.Dremio.EntityframeworkCore.Provider.Extensions;
using Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;
using Dino.Dremio.EntityframeworkCore.Provider.Storage;
using Dino.DremIO.Models;
using Dino.DremIO.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Dino.DremIO.Tests.EfCore;

public class DremioDataReaderTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static JobResultReponse BuildResult(
        IEnumerable<(string name, string type)> columns,
        IEnumerable<Dictionary<string, object>> rows)
    {
        return new JobResultReponse
        {
            RowCount = rows.Count(),
            Schema   = columns.Select(c => new Schema { Name = c.name, Type = new Dino.DremIO.Models.Type { Name = c.type } }).ToList(),
            Rows     = rows.ToList()
        };
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Read_AdvancesRows_ReturnsTrue_Until_Exhausted()
    {
        var result = BuildResult(
            [("Id", "INTEGER"), ("Name", "VARCHAR")],
            [
                new() { ["Id"] = 1, ["Name"] = "Dremio" },
                new() { ["Id"] = 2, ["Name"] = "Arrow" }
            ]);

        var reader = new DremioDataReader(result);

        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.False(reader.Read()); // exhausted
    }

    [Fact]
    public void FieldCount_Matches_Schema()
    {
        var result = BuildResult(
            [("Id", "INTEGER"), ("Name", "VARCHAR"), ("Score", "DOUBLE")],
            []);

        var reader = new DremioDataReader(result);

        Assert.Equal(3, reader.FieldCount);
    }

    [Fact]
    public void GetName_Returns_CorrectColumnName()
    {
        var result = BuildResult([("CustomerId", "BIGINT")], []);
        var reader = new DremioDataReader(result);

        Assert.Equal("CustomerId", reader.GetName(0));
    }

    [Fact]
    public void GetOrdinal_ReturnsCorrectIndex_CaseInsensitive()
    {
        var result = BuildResult([("Id", "INTEGER"), ("Name", "VARCHAR")], []);
        var reader = new DremioDataReader(result);

        Assert.Equal(0, reader.GetOrdinal("id"));     // lowercase
        Assert.Equal(1, reader.GetOrdinal("NAME"));   // uppercase
    }

    [Fact]
    public void GetValue_ReturnsExpected_AfterRead()
    {
        var result = BuildResult(
            [("Id", "INTEGER"), ("Name", "VARCHAR")],
            [new() { ["Id"] = 42, ["Name"] = "Alice" }]);

        var reader = new DremioDataReader(result);
        reader.Read();

        Assert.Equal(42, reader.GetValue(0));
        Assert.Equal("Alice", reader.GetValue(1));
    }

    [Fact]
    public void IsDBNull_True_When_ValueIsNull()
    {
        var result = BuildResult(
            [("Value", "VARCHAR")],
            [new() { ["Value"] = null! }]);

        var reader = new DremioDataReader(result);
        reader.Read();

        Assert.True(reader.IsDBNull(0));
    }

    [Fact]
    public void Indexer_ByOrdinal_And_ByName_ReturnSameValue()
    {
        var result = BuildResult(
            [("Score", "DOUBLE")],
            [new() { ["Score"] = 9.5 }]);

        var reader = new DremioDataReader(result);
        reader.Read();

        Assert.Equal(reader[0], reader["Score"]);
    }

    [Fact]
    public void HasRows_True_When_ResultHasRows()
    {
        var withRows    = BuildResult([("Id", "INTEGER")], [new() { ["Id"] = 1 }]);
        var withoutRows = BuildResult([("Id", "INTEGER")], []);

        Assert.True(new DremioDataReader(withRows).HasRows);
        Assert.False(new DremioDataReader(withoutRows).HasRows);
    }

    [Fact]
    public void GetFieldType_MapsToExpectedClrType()
    {
        var result = BuildResult(
            [
                ("I",  "INTEGER"),
                ("L",  "BIGINT"),
                ("D",  "DOUBLE"),
                ("B",  "BOOLEAN"),
                ("S",  "VARCHAR"),
                ("TS", "TIMESTAMP"),
            ], []);

        var reader = new DremioDataReader(result);

        Assert.Equal(typeof(int),      reader.GetFieldType(0));
        Assert.Equal(typeof(long),     reader.GetFieldType(1));
        Assert.Equal(typeof(double),   reader.GetFieldType(2));
        Assert.Equal(typeof(bool),     reader.GetFieldType(3));
        Assert.Equal(typeof(string),   reader.GetFieldType(4));
        Assert.Equal(typeof(DateTime), reader.GetFieldType(5));
    }
}

public class UseDremioExtensionTests
{
    private sealed class FakeDbContext(DbContextOptions options) : DbContext(options);

    [Fact]
    public void UseDremio_WithOption_RegistersDremioOptionsExtension()
    {
        var option = new DremIOOption
        {
            EndpointUrl = "http://localhost:9047",
            UserName    = "admin",
            Password    = "secret"
        };

        var options = new DbContextOptionsBuilder()
            .UseDremio(option)
            .Options;

        var extension = options.FindExtension<DremioOptionsExtension>();

        Assert.NotNull(extension);
        Assert.Equal("http://localhost:9047", extension.DremioOption.EndpointUrl);
        Assert.Equal("admin",                 extension.DremioOption.UserName);
    }

    [Fact]
    public void UseDremio_WithInlineCredentials_RegistersDremioOptionsExtension()
    {
        var options = new DbContextOptionsBuilder()
            .UseDremio("https://dremio.example.com", "user", "pass")
            .Options;

        var extension = options.FindExtension<DremioOptionsExtension>();

        Assert.NotNull(extension);
        Assert.Equal("https://dremio.example.com", extension.DremioOption.EndpointUrl);
    }

    [Fact]
    public void UseDremio_ExtensionInfo_IsDatabaseProvider()
    {
        var options = new DbContextOptionsBuilder()
            .UseDremio("http://localhost:9047", "u", "p")
            .Options;

        var extension = options.FindExtension<DremioOptionsExtension>()!;

        Assert.True(extension.Info.IsDatabaseProvider);
    }

    [Fact]
    public void UseDremio_LogFragment_ContainsDremio()
    {
        var options = new DbContextOptionsBuilder()
            .UseDremio("http://localhost:9047", "u", "p")
            .Options;

        var extension = options.FindExtension<DremioOptionsExtension>()!;

        Assert.Contains("Dremio", extension.Info.LogFragment);
    }
}
