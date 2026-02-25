using Dino.Dremio.EntityframeworkCore.Provider.Extensions;
using Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;
using Dino.Dremio.EntityframeworkCore.Provider.Storage;
using Dino.DremIO.Common;
using Dino.DremIO.Options;
using Dino.DremIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dino.DremIO.Tests.EfCore;

// ── DbContext dùng cho test ───────────────────────────────────────────────────

/// <summary>
/// DbContext tối giản để kiểm tra việc đăng ký DremioOptionsExtension.
/// </summary>
public class DremioTestDbContext : DbContext
{
    public DremioTestDbContext(DbContextOptions<DremioTestDbContext> options)
        : base(options) { }
}

/// <summary>Kết quả trả về từ <c>SELECT 1 AS Value</c>.</summary>
public class ScalarRow
{
    public int Value { get; set; }
}

// ── Integration Tests ─────────────────────────────────────────────────────────

/// <summary>
/// Integration tests kết nối thật tới DremIO qua REST API.
/// Cấu hình thông qua User Secrets (DremIOOption section).
///
/// Bộ test được chia làm hai phần:
///  A) Options wiring — kiểm tra UseDremio() đăng ký extension đúng cách.
///  B) ADO.NET / REST — kết nối và chạy SQL thật qua DremioDbConnection.
/// </summary>
[Trait("Category", "Integration")]
public class DremioDbContextIntegrationTests
{
    private readonly DremIOService _service;
    private readonly DremIOOption _option;

    public DremioDbContextIntegrationTests()
    {
        var provider = HostBuilderTest.Create((services, host) =>
        {
            services.AddHttpClient(Contants.DremIOClientKey).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            });
        }).Provider;
        _service = provider.GetRequiredService<DremIOService>();
        _option = provider.GetRequiredService<IOptions<DremIOOption>>().Value;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private DbContextOptions<DremioTestDbContext> BuildOptions() =>
        new DbContextOptionsBuilder<DremioTestDbContext>()
            .UseDremio(_option, _service)
            .Options;

    /// <summary>Opens a real DremioDbConnection (bypasses full EF Core pipeline).</summary>
    private DremioDbConnection OpenConnection()
    {
        var conn = new DremioDbConnection(_service, _option);
        conn.Open();
        return conn;
    }

    // ── A: Options wiring ─────────────────────────────────────────────────────

    [Fact]
    public void Options_UseDremio_RegistersDremioExtension()
    {
        var ext = BuildOptions().FindExtension<DremioOptionsExtension>();

        Assert.NotNull(ext);
        Assert.Equal(_option.EndpointUrl, ext.DremioOption.EndpointUrl);
        Assert.Same(_service, ext.DremioService);
    }

    [Fact]
    public void Options_Extension_IsDatabaseProvider()
    {
        var ext = BuildOptions().FindExtension<DremioOptionsExtension>()!;
        Assert.True(ext.Info.IsDatabaseProvider);
    }

    [Fact]
    public void Options_LogFragment_ContainsDremio()
    {
        var ext = BuildOptions().FindExtension<DremioOptionsExtension>()!;
        Assert.Contains("Dremio", ext.Info.LogFragment);
    }

    // ── B: DremioDbConnection — không cần full EF Core pipeline ──────────────

    [Fact]
    public void Connection_Open_StateBecomesOpen()
    {
        using var conn = new DremioDbConnection(_service, _option);

        Assert.Equal(System.Data.ConnectionState.Closed, conn.State);
        conn.Open();
        Assert.Equal(System.Data.ConnectionState.Open, conn.State);
    }

    [Fact]
    public async Task Connection_OpenAsync_StateBecomesOpen()
    {
        await using var conn = new DremioDbConnection(_service, _option);

        await conn.OpenAsync();
        Assert.Equal(System.Data.ConnectionState.Open, conn.State);
    }

    [Fact]
    public void Connection_Database_ReturnsExpectedValue()
    {
        using var conn = new DremioDbConnection(_service, _option);
        Assert.Equal("DremIO", conn.Database);
    }

    [Fact]
    public void Connection_DataSource_MatchesEndpointUrl()
    {
        using var conn = new DremioDbConnection(_service, _option);
        Assert.Equal(_option.EndpointUrl, conn.DataSource);
    }

    [Fact]
    public void Connection_ConnectionString_ContainsEndpoint()
    {
        using var conn = new DremioDbConnection(_service, _option);
        Assert.Contains(_option.EndpointUrl!, conn.ConnectionString);
    }

    [Fact]
    public void Connection_CreateDremioCommand_ReturnsCommand()
    {
        using var conn = new DremioDbConnection(_service, _option);
        using var cmd = conn.CreateDremioCommand();
        Assert.IsType<DremioDbCommand>(cmd);
    }

    // ── C: SQL thật qua DremIO REST API ──────────────────────────────────────

    [Fact]
    public async Task Command_Select1_ExecuteScalar_ReturnsOne()
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateDremioCommand();
        // VALUES is fully supported by Dremio without a FROM clause
        cmd.CommandText = "VALUES (1)";

        var result = await cmd.ExecuteScalarAsync();

        Assert.NotNull(result);
        Assert.Equal(1, Convert.ToInt32(result));
    }

    [Fact]
    public async Task Command_Select1_ExecuteReader_ReadsSingleRow()
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateDremioCommand();
        cmd.CommandText = "VALUES (1)";

        await using var reader = await cmd.ExecuteReaderAsync();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(1, Convert.ToInt32(reader.GetValue(0)));
        Assert.False(await reader.ReadAsync()); // chỉ 1 hàng
    }

    [Fact]
    public async Task Command_UnionAll_ExecuteReader_ReadsAllRows()
    {
        using var conn = OpenConnection();
        var cmd = conn.CreateDremioCommand();
        // VALUES with multiple rows — Dremio returns them as EXPR$0
        cmd.CommandText = "VALUES (1), (2), (3)";

        await using var reader = await cmd.ExecuteReaderAsync();

        var rows = new List<int>();
        while (await reader.ReadAsync())
            rows.Add(Convert.ToInt32(reader.GetValue(0)));

        Assert.Equal(3, rows.Count);
    }

    // ── D: DbContext.Database.GetDbConnection() ──────────────────────────────

    [Fact]
    public void DbContext_GetDbConnection_ReturnsDremioDbConnection()
    {
        using var ctx = new DremioTestDbContext(BuildOptions());
        var conn = ctx.Database.GetDbConnection();

        Assert.NotNull(conn);
        Assert.IsType<DremioDbConnection>(conn);
    }

    [Fact]
    public void DbContext_GetDbConnection_ConnectionStringContainsEndpoint()
    {
        using var ctx = new DremioTestDbContext(BuildOptions());
        var conn = ctx.Database.GetDbConnection();

        // The connection string is built by DremioDbConnection from DremIOOption
        Assert.Contains(_option.EndpointUrl, conn.ConnectionString);
    }

    [Fact]
    public async Task DbContext_GetDbConnection_Open_ExecuteScalar_ReturnsValue()
    {
        await using var ctx = new DremioTestDbContext(BuildOptions());
        var conn = ctx.Database.GetDbConnection();

        await conn.OpenAsync();
        try
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "VALUES (42)";
            var result = await cmd.ExecuteScalarAsync();
            Assert.Equal(42, Convert.ToInt32(result));
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
}
