using Dino.Dremio.EntityframeworkCore.Provider.Query.TranslateMethods;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.Dremio.EntityframeworkCore.Provider.Query
{
    public class DremioMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public DremioMethodCallTranslatorProvider(
        RelationalMethodCallTranslatorProviderDependencies dependencies)
        : base(dependencies)
        {
            AddTranslators(new IMethodCallTranslator[]
            {
            new DremioStringMethodTranslator(dependencies.SqlExpressionFactory)
            });
        }
    }
}
