using Dapper.FluentMap.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Data.POCO.MySql
{
    public class MySqlColumn : IColumn
    {
        public string Table { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    internal class MySqlColumnMap : EntityMap<MySqlColumn>, IColumnMap
    {
        internal MySqlColumnMap()
        {
            Map(t => t.Table).ToColumn("table_name");
            Map(t => t.Name).ToColumn("column_name");
        }
    }
}
