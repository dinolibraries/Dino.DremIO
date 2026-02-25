using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.Dremio.EntityframeworkCore.Provider.Attributes
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public class TableContextAttribute : Attribute
    {
        public TableContextAttribute(params string[] contexts)
        {
            Contexts = contexts;
        }
        public string[] Contexts { get; set; }
    }
}
