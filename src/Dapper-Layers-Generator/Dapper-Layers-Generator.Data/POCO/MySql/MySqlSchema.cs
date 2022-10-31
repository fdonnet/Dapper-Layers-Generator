using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Data.POCO.MySql
{
    public class MySqlSchema : ISchema
    {
        public string Name { get; set; } = null!;
        public IList<ITable>? Tables { get; set; }
    }

}

