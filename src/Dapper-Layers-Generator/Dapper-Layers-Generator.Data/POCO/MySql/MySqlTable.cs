using Dapper.FluentMap.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dapper_Layers_Generator.Data.POCO.MySql
{
    [Table("TABLES")]
    public class MySqlTable :ITable
    {
        public string Schema { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    internal class MySqlTableMap : EntityMap<MySqlTable> , ITableMap
    {
        internal MySqlTableMap()
        {
            Map(t => t.Schema).ToColumn("TABLE_SCHEMA");
            Map(t => t.Name).ToColumn("TABLE_NAME");
        }
    }
}
