using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dino.Dremio.EntityframeworkCore.Provider.Query;

/// <summary>
/// Dremio-specific <see cref="SqlExpressionFactory"/>.
/// Dremio is largely ANSI compliant; this subclass can be extended to handle
/// vendor-specific functions (e.g. CONVERT_TIMEZONE, FLATTEN, etc.).
/// </summary>
public sealed class DremioSqlExpressionFactory : SqlExpressionFactory
{
    public DremioSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
        : base(dependencies) { }

    // ── Dremio function helpers ─────────────────────────────────────────────
    // Add Dremio-specific SQL function wrappers here as needed.
    // Example:
    //
    //   public SqlFunctionExpression ConvertTimezone(
    //       SqlExpression timestamp, SqlExpression timezone) =>
    //       Function("CONVERT_TIMEZONE", new[] { timezone, timestamp },
    //           nullable: true, argumentsPropagateNullability: new[] { false, true },
    //           typeof(DateTime));
}
