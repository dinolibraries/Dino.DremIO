using Dino.DremIO.Options;
using Dino.DremIO.Services;
using System.Data;
using System.Data.Common;

namespace Dino.Dremio.EntityframeworkCore.Provider.Storage;

/// <summary>
/// A <see cref="DbConnection"/> facade that delegates query execution to the
/// DremIO REST client (<see cref="DremIOService"/>). EF Core never opens a real
/// TCP connection; instead, <see cref="DremioDbCommand"/> calls the HTTP API.
/// </summary>
public sealed class DremioDbConnection : DbConnection
{
    private readonly DremIOService _dremioService;
    private readonly DremIOOption _option;
    private ConnectionState _state = ConnectionState.Closed;

    /// <summary>
    /// Optional lookup: table name (case-insensitive) → Dremio catalog context paths.
    /// Populated by <see cref="DremioRelationalConnection"/> from the EF Core model
    /// annotations written by <see cref="../Infrastructure/DremioTableContextConvention"/>.
    /// </summary>
    internal IReadOnlyDictionary<string, string[]>? TableContexts { get; }

    public DremioDbConnection(
        DremIOService dremioService,
        DremIOOption option,
        IReadOnlyDictionary<string, string[]>? tableContexts = null)
    {
        _dremioService = dremioService;
        _option = option;
        TableContexts = tableContexts;
    }

    // ── DbConnection overrides ──────────────────────────────────────────────

    public override string ConnectionString
    {
        get => $"Endpoint={_option.EndpointUrl};User={_option.UserName}";
        set { /* read-only; settings come from DremIOOption */ }
    }

    public override string Database => "DremIO";
    public override string DataSource => _option.EndpointUrl ?? string.Empty;
    public override string ServerVersion => "DremIO";
    public override ConnectionState State => _state;

    public override void Open()
    {
        // No real socket — just mark as open.
        _state = ConnectionState.Open;
    }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        _state = ConnectionState.Open;
        return Task.CompletedTask;
    }

    public override void Close()
    {
        _state = ConnectionState.Closed;
    }

    public override void ChangeDatabase(string databaseName) { /* no-op */ }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
        throw new NotSupportedException("DremIO does not support transactions.");

    protected override DbCommand CreateDbCommand() =>
        new DremioDbCommand(_dremioService, this);

    // ── Convenience ─────────────────────────────────────────────────────────

    /// <summary>Creates a typed command already bound to this connection.</summary>
    public DremioDbCommand CreateDremioCommand() =>
        new(_dremioService, this);
}
