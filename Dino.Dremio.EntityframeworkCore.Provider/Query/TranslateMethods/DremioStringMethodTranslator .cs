using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dino.Dremio.EntityframeworkCore.Provider.Query.TranslateMethods
{
    public class DremioStringMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _contains =
        typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

        private static readonly MethodInfo _startsWith =
            typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!;

        private static readonly MethodInfo _endsWith =
            typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!;

        private readonly ISqlExpressionFactory _sql;

        public DremioStringMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sql = sqlExpressionFactory;
        }

        public SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (instance == null)
                return null;

            if (method.Equals(_contains))
                return BuildLike(instance, arguments[0], "%", "%");

            if (method.Equals(_startsWith))
                return BuildLike(instance, arguments[0], "", "%");

            if (method.Equals(_endsWith))
                return BuildLike(instance, arguments[0], "%", "");

            return null;
        }

        private SqlExpression BuildLike(
            SqlExpression column,
            SqlExpression value,
            string prefix,
            string suffix)
        {
            SqlExpression pattern;

            if (value is SqlConstantExpression constant)
            {
                var text = constant.Value?.ToString() ?? "";
                pattern = _sql.Constant($"{prefix}{text}{suffix}");
            }
            else
            {
                // parameter case
                if (prefix == "%" && suffix == "%")
                {
                    pattern = _sql.Add(
                        _sql.Add(_sql.Constant("%"), value),
                        _sql.Constant("%"));
                }
                else if (prefix == "")
                {
                    pattern = _sql.Add(value, _sql.Constant("%"));
                }
                else
                {
                    pattern = _sql.Add(_sql.Constant("%"), value);
                }
            }

            return _sql.Like(column, pattern);
        }
    }
}
