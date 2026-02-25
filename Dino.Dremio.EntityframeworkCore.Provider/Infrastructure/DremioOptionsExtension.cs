using Dino.Dremio.EntityframeworkCore.Provider.Query;
using Dino.Dremio.EntityframeworkCore.Provider.Storage;
using Dino.DremIO.Options;
using Dino.DremIO.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;

namespace Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;

/// <summary>
/// Registers DremIO-specific EF Core services and stores the connection options
/// that were supplied via <c>UseDremio()</c>.
/// </summary>
public sealed class DremioOptionsExtension : IDbContextOptionsExtension
{
    private DremIOOption _dremioOption = new();
    private DremIOService? _dremioService;
    private DbContextOptionsExtensionInfo? _info;

    public DremioOptionsExtension() { }

    // Copy-constructor used by EF Core's With* pattern.
    private DremioOptionsExtension(DremioOptionsExtension copyFrom)
    {
        _dremioOption = copyFrom._dremioOption;
        _dremioService = copyFrom._dremioService;
    }

    // ── Public configuration ────────────────────────────────────────────────

    public DremIOOption DremioOption => _dremioOption;
    public DremIOService? DremioService => _dremioService;

    public DremioOptionsExtension WithDremioOption(DremIOOption option)
    {
        var clone = Clone();
        clone._dremioOption = option;
        return clone;
    }

    public DremioOptionsExtension WithDremioService(DremIOService service)
    {
        var clone = Clone();
        clone._dremioService = service;
        return clone;
    }

    // ── IDbContextOptionsExtension ──────────────────────────────────────────

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        // Make DremIOOption available to the EF Core internal service provider.
        services.AddSingleton(_dremioOption);

        // If a pre-built DremIOService was supplied, register it directly.
        if (_dremioService is not null)
        {
            services.AddSingleton(_dremioService);
        }
        // Register all Dremio-specific EF Core services.
        new EntityFrameworkRelationalServicesBuilder(services)
            // ── Core provider identity ──────────────────────────────────────
            .TryAdd<IDatabaseProvider, DatabaseProvider<DremioOptionsExtension>>()
            .TryAdd<LoggingDefinitions, DremioLoggingDefinitions>()
            // ── SQL generation ──────────────────────────────────────────────
            .TryAdd<ISqlGenerationHelper, DremioSqlGenerationHelper>()
            .TryAdd<IUpdateSqlGenerator, DremioUpdateSqlGenerator>()
            .TryAdd<IModificationCommandBatchFactory, DremioModificationCommandBatchFactory>()
            .TryAdd<IMigrationsSqlGenerator, DremioMigrationsSqlGenerator>()
            // ── Connection / type system ────────────────────────────────────
            .TryAdd<IRelationalConnection, DremioRelationalConnection>()
            .TryAdd<IRelationalTypeMappingSource, DremioTypeMappingSource>()
            .TryAdd<IRelationalDatabaseCreator, DremioRelationalDatabaseCreator>()
            // ── Query pipeline ──────────────────────────────────────────────
            .TryAdd<IQuerySqlGeneratorFactory, DremioQuerySqlGeneratorFactory>()
            .TryAdd<ISqlExpressionFactory, DremioSqlExpressionFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, DremioQueryTranslationPostprocessorFactory>()
            // ── Execution strategy ──────────────────────────────────────────
            .TryAdd<IExecutionStrategyFactory, DremioExecutionStrategyFactory>()
            // ── Conventions ─────────────────────────────────────────────────
            .TryAdd<IConventionSetBuilder, DremioConventionSetBuilder>()
            // ── Register all remaining EF Core relational core services ─────
            .TryAddCoreServices();
    }

    public void Validate(IDbContextOptions options) { /* nothing to validate yet */ }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private DremioOptionsExtension Clone() => new(this);

    // ── Inner: ExtensionInfo ─────────────────────────────────────────────────

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }

        private new DremioOptionsExtension Extension => (DremioOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider => true;

        public override string LogFragment => "using Dremio ";

        public override int GetServiceProviderHashCode() =>
            Extension._dremioOption.EndpointUrl?.GetHashCode() ?? 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) =>
            other is ExtensionInfo otherInfo &&
            otherInfo.Extension._dremioOption.EndpointUrl == Extension._dremioOption.EndpointUrl;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Dremio:EndpointUrl"] = Extension._dremioOption.EndpointUrl ?? "(null)";
        }
    }
}
