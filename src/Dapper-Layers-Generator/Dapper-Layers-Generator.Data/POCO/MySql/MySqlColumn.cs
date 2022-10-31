using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Dapper_Layers_Generator.Data.POCO.MySql
{
    public class MySqlColumn : IColumn
    {
        public string Schema { get; set; } = null!;
        public string Table { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Position { get; set; }
        public bool IsNullable { get; set; }
        public string DataType { get; set; } = null!;
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsPrimary { get; set; }
        public bool HasUniqueIndex { get; set; }
        public bool IsAutoIncrement { get; set; }
    }
}
