using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;

/// <summary>
/// Builds the convention set used when the DremIO provider is configured.
/// Applies the base relational conventions and then trims any conventions
/// that are not applicable to a read-only / REST-based provider.
/// </summary>
public sealed class DremioConventionSetBuilder : RelationalConventionSetBuilder, IConventionSetBuilder
{
    public DremioConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies) { }

    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        // Remove conventions that require DDL support (DremIO is typically
        // used as a query engine, not for DDL operations).
        // conventionSet.ModelFinalizingConventions.Remove(...);

        return conventionSet;
    }
}
