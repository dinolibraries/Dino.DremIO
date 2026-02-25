using Dino.DremIO.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;

/// <summary>
/// A minimal <see cref="RelationalOptionsExtension"/> that satisfies EF Core's
/// internal check inside <c>RelationalDatabaseFacadeExtensions</c>:
/// <c>options.Extensions.OfType&lt;RelationalOptionsExtension&gt;().Any()</c>.
///
/// EF Core's relational APIs (e.g. <c>Database.GetDbConnection()</c>,
/// <c>Database.ExecuteSqlRaw()</c>) require at least one
/// <see cref="RelationalOptionsExtension"/> to be present in the options.
/// All actual DremIO settings live in <see cref="DremioOptionsExtension"/>;
/// this class only exposes the connection string EF Core needs for logging.
/// </summary>
public sealed class DremioRelationalOptionsExtension : RelationalOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    public DremioRelationalOptionsExtension() { }

    private DremioRelationalOptionsExtension(DremioRelationalOptionsExtension copyFrom)
        : base(copyFrom) { }

    protected override RelationalOptionsExtension Clone() => new DremioRelationalOptionsExtension(this);

    public override DbContextOptionsExtensionInfo Info =>
        _info ??= new ExtensionInfo(this);

    public override void ApplyServices(IServiceCollection services)
    {
        // No extra services needed — DremioOptionsExtension handles all registrations.
    }

    // ── Inner: ExtensionInfo ──────────────────────────────────────────────────

    private sealed class ExtensionInfo : RelationalExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }

        public override bool IsDatabaseProvider => false; // DremioOptionsExtension is the provider

        public override string LogFragment => string.Empty;

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }
    }
}
