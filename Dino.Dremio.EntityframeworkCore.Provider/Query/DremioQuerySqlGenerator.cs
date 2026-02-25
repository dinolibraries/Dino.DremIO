using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Dino.Dremio.EntityframeworkCore.Provider.Query;

/// <summary>
/// Generates Dremio-flavoured SQL from the EF Core query model.
/// Dremio is largely ANSI-SQL compliant so this thin subclass of the
/// default <see cref="QuerySqlGenerator"/> just overrides the few spots
/// where Dremio deviates (e.g. no TOP, uses LIMIT/OFFSET).
/// </summary>
public sealed class DremioQuerySqlGenerator : QuerySqlGenerator
{
    public DremioQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
        : base(dependencies) { }

    // Dremio uses standard ANSI LIMIT / OFFSET, which is already emitted by the
    // base class, so no override is required for paging.

    /// <summary>
    /// Dremio does not support the LIKE escape character clause.
    /// Strip the ESCAPE part from LIKE expressions.
    /// </summary>
    protected override Expression VisitLike(LikeExpression likeExpression)
    {
        // Re-visit without an escape character (Dremio ignores it anyway).
        if (likeExpression.EscapeChar is not null)
        {
            likeExpression = new LikeExpression(
                likeExpression.Match,
                likeExpression.Pattern,
                escapeChar: null,
                likeExpression.TypeMapping);
        }
        return base.VisitLike(likeExpression);
    }
}
