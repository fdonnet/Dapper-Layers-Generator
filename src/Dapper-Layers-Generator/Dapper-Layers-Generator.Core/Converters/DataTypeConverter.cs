using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Converters
{
    public interface IDataTypeConverter
    {
        string GetDotNetDataType(string sqlDataTypeName, bool nullable = false);
    }

    public abstract class DataTypeConverter
    {
        public string GetDotNetDataType(string sqlDataTypeName, bool nullable = false)
        {
            return sqlDataTypeName == null
                ? throw new ArgumentNullException(nameof(sqlDataTypeName))
                : sqlDataTypeName.ToLower() switch
            {
                "bigint" => "long" + (nullable ? "?" : ""),
                "binary" or "image" or "varbinary" => "byte[]" + (nullable ? "?" : ""),
                "bit" => "bool" + (nullable ? "?" : ""),
                "char" => "char" + (nullable ? "?" : ""),
                "datetime" or "smalldatetime" => "DateTime" + (nullable ? "?" : ""),
                "decimal" or "money" or "numeric" => "decimal" + (nullable ? "?" : ""),
                "float" => "double" + (nullable ? "?" : ""),
                "int" => "int" + (nullable ? "?" : ""),
                "nchar" or "nvarchar" or "text" or "varchar" or "xml" => "string" + (nullable ? "?" : ""),
                "real" => "single" + (nullable ? "?" : ""),
                "smallint" => "short" + (nullable ? "?" : ""),
                "tinyint" => "byte" + (nullable ? "?" : ""),
                "uniqueidentifier" => "System.Guid" + (nullable ? "?" : ""),
                "date" => "DateTime" + (nullable ? "?" : ""),
                "rowversion" => "byte[]",
                _ => "object",
            };
        }
    }
}
