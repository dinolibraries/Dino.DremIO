using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Dino.Dremio.EntityframeworkCore.Provider.Query;

/// <summary>
/// Creates <see cref="DremioQuerySqlGenerator"/> instances for each query.
/// Registered as <see cref="IQuerySqlGeneratorFactory"/> in the DI container.
/// </summary>
public sealed class DremioQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
{
    private readonly QuerySqlGeneratorDependencies _dependencies;

    public DremioQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    public QuerySqlGenerator Create() => new DremioQuerySqlGenerator(_dependencies);
}
