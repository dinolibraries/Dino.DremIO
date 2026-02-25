using Dino.Dremio.EntityframeworkCore.Provider.Attributes;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Dino.Dremio.EntityframeworkCore.Provider.Infrastructure;

/// <summary>
/// Reads <see cref="TableContextAttribute"/> from each entity CLR type and
/// stores the context path(s) as the model annotation <c>"DremIO:Contexts"</c>.
/// <para>
/// This allows the rest of the EF Core pipeline (including
/// <see cref="Storage.DremioRelationalConnection"/> and
/// <see cref="Storage.DremioDbCommand"/>) to resolve the correct Dremio
/// catalog context for a given table without any user-level plumbing.
/// </para>
/// </summary>
public sealed class DremioTableContextConvention : IModelFinalizingConvention
{
    /// <summary>Annotation key stored on each <c>IEntityType</c>.</summary>
    public const string AnnotationKey = "DremIO:Contexts";

    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var attr = entityType.ClrType?
                .GetCustomAttributes(typeof(TableContextAttribute), inherit: true)
                .OfType<TableContextAttribute>()
                .FirstOrDefault();

            if (attr?.Contexts is { Length: > 0 } contexts)
                // Store as comma-separated string in the model annotation.
                entityType.Builder.HasAnnotation(AnnotationKey, string.Join(",", contexts));
        }
    }
}
