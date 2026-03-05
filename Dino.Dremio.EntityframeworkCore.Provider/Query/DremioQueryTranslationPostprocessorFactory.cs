using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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
    private readonly RelationalQueryTranslationPostprocessorDependencies _relationalDependencies;

    public DremioQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
#if NET9_0_OR_GREATER
        RelationalQueryCompilationContext queryCompilationContext)
#else
        QueryCompilationContext queryCompilationContext)
#endif
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _relationalDependencies = relationalDependencies;
    }

    // Rewrite CLR `string.Contains(...)` into the Dremio `CONTAINS(column, value)` SQL call
    public override Expression Process(Expression query)
    {
        query = base.Process(query);
        var rewriter = new ContainsToSqlRewriter(_relationalDependencies.SqlExpressionFactory);
        return rewriter.Visit(query)!;
    }

    private sealed class ContainsToSqlRewriter : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public ContainsToSqlRewriter(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitExtension(Expression node)
        {
            // ShapedQueryExpression throws from VisitChildren; handle manually.
            if (node is ShapedQueryExpression shaped)
            {
                var visitedQuery = Visit(shaped.QueryExpression);
                var visitedShaper = Visit(shaped.ShaperExpression);

                if (visitedQuery == shaped.QueryExpression && visitedShaper == shaped.ShaperExpression)
                {
                    return node;
                }

                return new ShapedQueryExpression(visitedQuery, visitedShaper);
            }

            return base.VisitExtension(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // instance method: someString.Contains(value)
            if (node.Method.Name == "Contains" && node.Method.DeclaringType == typeof(string) && node.Object is not null && node.Arguments.Count == 1)
            {
                var visitedInstance = Visit(node.Object);
                var visitedArg = Visit(node.Arguments[0]);

                if (visitedInstance is SqlExpression instanceSql && visitedArg is SqlExpression argSql)
                {
                    return _sqlExpressionFactory.Function(
                        "CONTAINS",
                        new[] { instanceSql, argSql },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        typeof(bool));
                }
            }
            return base.VisitMethodCall(node);
        }
    }
}
