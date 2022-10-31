using Dapper.FluentMap.Mapping;
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
        public IEnumerable<ITable>? Tables { get; set; }
    }


    internal class MySqlSchemaMap : EntityMap<MySqlSchema>, ISchemaMap
    {
        internal MySqlSchemaMap()
        {
            Map(t => t.Name).ToColumn("schema_name");
        }
    }
}

