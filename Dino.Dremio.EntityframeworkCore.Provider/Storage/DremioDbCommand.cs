using Dino.DremIO.Models;
using Dino.DremIO.Services;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Dino.Dremio.EntityframeworkCore.Provider.Storage;

/// <summary>
/// A <see cref="DbCommand"/> implementation that executes SQL queries against
/// the DremIO REST API via <see cref="DremIOService"/>.
/// </summary>
public sealed class DremioDbCommand : DbCommand
{
    private readonly DremIOService _dremioService;
    private string[] _contexts = Array.Empty<string>();
    private string _commandText = string.Empty;
    private DremioDbConnection? _connection;
    private DremioDbParameterCollection _parameters = new();

    public DremioDbCommand(DremIOService dremioService, DremioDbConnection connection)
    {
        _dremioService = dremioService;
        _connection = connection;
    }

    // ── DbCommand overrides ─────────────────────────────────────────────────

    public override string CommandText
    {
        get => _commandText;
        set => _commandText = value ?? string.Empty;
    }

    public override int CommandTimeout { get; set; } = 300;
    public override CommandType CommandType { get; set; } = CommandType.Text;
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }

    protected override DbConnection? DbConnection
    {
        get => _connection;
        set => _connection = value as DremioDbConnection;
    }

    protected override DbTransaction? DbTransaction { get; set; }

    protected override DbParameterCollection DbParameterCollection => _parameters;

    protected override DbParameter CreateDbParameter() => new DremioDbParameter();

    // ── Execution ───────────────────────────────────────────────────────────

    public override int ExecuteNonQuery() =>
        ExecuteNonQueryAsync(CancellationToken.None).GetAwaiter().GetResult();

    public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        var sql = ApplyParameters(_commandText);
        var ctx = _dremioService.CreateContext(ResolveContexts(sql));
        var jobId = await ctx.QueryAsync(sql, cancellationToken);
        if (string.IsNullOrEmpty(jobId))
            throw new InvalidOperationException("DremIO returned no job ID for a non-query command.");

        var job = _dremioService.CreateJob();
        var result = await job.WaitAsync(jobId, CommandTimeout, cancellationToken);
        return result.RowCount;
    }

    public override object? ExecuteScalar() =>
        ExecuteScalarAsync(CancellationToken.None).GetAwaiter().GetResult();

    public override async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        using var reader = await ExecuteDbDataReaderAsync(CommandBehavior.Default, cancellationToken);
        if (await reader.ReadAsync(cancellationToken) && reader.FieldCount > 0)
            return reader.GetValue(0);
        return null;
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) =>
        ExecuteDbDataReaderAsync(behavior, CancellationToken.None).GetAwaiter().GetResult();

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(
        CommandBehavior behavior, CancellationToken cancellationToken)
    {
        var sql = ApplyParameters(_commandText);
        var ctx = _dremioService.CreateContext(ResolveContexts(sql));
        var jobId = await ctx.QueryAsync(sql, cancellationToken);
        if (string.IsNullOrEmpty(jobId))
            throw new InvalidOperationException("DremIO returned no job ID.");

        var job = _dremioService.CreateJob();
        var jobResult = await job.WaitAsync(jobId, CommandTimeout, cancellationToken);
        if (jobResult.JobState != JobState.COMPLETED)
            throw new InvalidOperationException($"DremIO job ended with state: {jobResult.JobState}. {jobResult.ErrorMessage}");

        var resultData = await job.ResultAsync(jobId, cancellationToken: cancellationToken);
        var converted = new JobResultReponse
        {
            RowCount = resultData?.RowCount ?? 0,
            Schema   = resultData?.Schema   ?? new(),
            Rows     = resultData?.Rows     ?? new()
        };
        return new DremioDataReader(converted);
    }

    public override void Prepare() { /* no-op for a REST-based driver */ }

    public override void Cancel() { /* cancellation not supported */ }

    // ── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Explicitly sets the Dremio catalog contexts (e.g. space / folder path).
    /// When set, these take priority over the contexts discovered automatically
    /// from <see cref="Attributes.TableContextAttribute"/> on the entity type.
    /// </summary>
    public void SetContexts(params string[] contexts) => _contexts = contexts;

    /// <summary>
    /// Returns the contexts to use for a query. Explicit contexts (<see cref="SetContexts"/>)
    /// take priority; otherwise the table name is extracted from the SQL and looked up
    /// in the <see cref="DremioDbConnection.TableContexts"/> dictionary built from
    /// <see cref="Attributes.TableContextAttribute"/> model annotations.
    /// </summary>
    private string[] ResolveContexts(string sql)
    {
        // Explicit always wins.
        if (_contexts.Length > 0) return _contexts;

        var lookup = _connection?.TableContexts;
        if (lookup is null || lookup.Count == 0) return Array.Empty<string>();

        // EF Core generates: FROM "table-name" AS "alias"
        // We match the first FROM clause table name (double-quoted identifier).
        var match = Regex.Match(sql, @"\bFROM\s+""([^""]+)""", RegexOptions.IgnoreCase);
        if (match.Success && lookup.TryGetValue(match.Groups[1].Value, out var ctx))
            return ctx;

        return Array.Empty<string>();
    }

    /// <summary>
    /// Naive parameter substitution: replaces @name placeholders with their
    /// literal values so the SQL string can be sent to the DremIO REST API.
    /// </summary>
    private string ApplyParameters(string sql)
    {
        foreach (DremioDbParameter p in _parameters)
        {
            var literal = p.Value switch
            {
                null => "NULL",
                string s => $"'{s.Replace("'", "''")}'",
                bool b => b ? "TRUE" : "FALSE",
                DateTime dt => $"TIMESTAMP '{dt:yyyy-MM-dd HH:mm:ss}'",
                _ => p.Value.ToString() ?? "NULL"
            };
            sql = sql.Replace(p.ParameterName, literal, StringComparison.OrdinalIgnoreCase);
        }
        return sql;
    }
}
