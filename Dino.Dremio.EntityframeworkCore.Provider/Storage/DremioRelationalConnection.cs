using Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;
using Dino.DremIO.Options;
using Dino.DremIO.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace Dino.Dremio.EntityframeworkCore.Provider.Storage;

/// <summary>
/// EF Core's <see cref="IRelationalConnection"/> for DremIO.
/// Wraps <see cref="DremioDbConnection"/> (the "fake" ADO.NET connection) and
/// integrates it with the EF Core relational pipeline.
/// </summary>
public sealed class DremioRelationalConnection : RelationalConnection
{
    private readonly DremIOService _dremioService;
    private readonly DremIOOption _dremioOption;
    private readonly ICurrentDbContext _currentContext;

    public DremioRelationalConnection(
        RelationalConnectionDependencies dependencies,
        DremIOService dremioService,
        DremIOOption dremioOption,
        ICurrentDbContext currentContext)
        : base(dependencies)
    {
        _dremioService = dremioService;
        _dremioOption  = dremioOption;
        _currentContext = currentContext;
    }

    /// <summary>
    /// EF Core calls this to create the underlying ADO.NET connection.
    /// Builds a table-name → contexts lookup from the EF Core model (populated
    /// by <see cref="DremioTableContextConvention"/>) and passes it along so
    /// <see cref="DremioDbCommand"/> can auto-set the correct Dremio catalog
    /// context for every query without any user-level plumbing.
    /// </summary>
    protected override DbConnection CreateDbConnection()
    {
        var tableContexts = BuildTableContextMap(_currentContext.Context.Model);
        return new DremioDbConnection(_dremioService, _dremioOption, tableContexts);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Dictionary<string, string[]> BuildTableContextMap(IModel model)
    {
        var map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var entityType in model.GetEntityTypes())
        {
            var annotation = entityType.FindAnnotation(DremioTableContextConvention.AnnotationKey);
            if (annotation?.Value is string csv && !string.IsNullOrEmpty(csv))
            {
                var tableName = entityType.GetTableName();
                if (tableName is not null)
                    map[tableName] = csv.Split(',');
            }
        }
        return map;
    }
}
