using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Data.POCO
{
    public interface IColumn
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public bool IsNullable { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsPrimary { get; set; }
        public List<string>? UniqueIndexNames { get; set; }
        public bool IsAutoIncrement { get; set; }

    }

}
