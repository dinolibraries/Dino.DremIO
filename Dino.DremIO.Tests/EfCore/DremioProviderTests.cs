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
            Schema = columns.Select(c => new Schema { Name = c.name, Type = new Dino.DremIO.Models.Type { Name = c.type } }).ToList(),
            Rows = rows.ToList()
        };
    }

    // ── Tests ─────────────────────────────────────────────────────────────────
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
