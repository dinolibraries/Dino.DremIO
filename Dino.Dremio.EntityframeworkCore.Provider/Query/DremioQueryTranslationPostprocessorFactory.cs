using Microsoft.EntityFrameworkCore.Query;

namespace Dino.Dremio.EntityframeworkCore.Provider.Query;

/// <summary>
/// Post-processes the relational query model after translation, applying
/// Dremio-specific rewrites (e.g. flattening sub-queries, inlining views).
/// The base implementation from EF Core handles the common cases; override
/// <see cref="Process"/> to add Dremio-specific transforms.
/// </summary>
public sealed class DremioQueryTranslationPostprocessorFactory
    : IQueryTranslationPostprocessorFactory
{
    private readonly QueryTranslationPostprocessorDependencies _dependencies;
    private readonly RelationalQueryTranslationPostprocessorDependencies _relationalDependencies;

    public DremioQueryTranslationPostprocessorFactory(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies)
    {
        _dependencies = dependencies;
        _relationalDependencies = relationalDependencies;
    }

    public QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext) =>
        new DremioQueryTranslationPostprocessor(
            _dependencies,
            _relationalDependencies,
#if NET9_0_OR_GREATER
            (RelationalQueryCompilationContext)queryCompilationContext);
#else
            queryCompilationContext);
#endif
}

/// <summary>
/// The concrete postprocessor. Extend <see cref="Process"/> to add
/// Dremio-specific expression rewrites.
/// </summary>
internal sealed class DremioQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
{
    public DremioQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
#if NET9_0_OR_GREATER
        RelationalQueryCompilationContext queryCompilationContext)
#else
        QueryCompilationContext queryCompilationContext)
#endif
        : base(dependencies, relationalDependencies, queryCompilationContext) { }

    // Override Process() here to add Dremio-specific expression tree rewrites.
}
