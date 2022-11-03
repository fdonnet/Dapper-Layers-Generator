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
            if (sqlDataTypeName == null) throw new ArgumentNullException(nameof(sqlDataTypeName));
            switch (sqlDataTypeName.ToLower())
            {
                case "bigint":
                    return "long" + (nullable ? "?" : "");
                case "binary":
                case "image":
                case "varbinary":
                    return "byte[]";
                case "bit":
                    return "bool" + (nullable ? "?" : "");
                case "char":
                    return "char" + (nullable ? "?" : "");
                case "datetime":
                case "smalldatetime":
                    return "System.DateTime" + (nullable ? "?" : "");
                case "decimal":
                case "money":
                case "numeric":
                    return "decimal" + (nullable ? "?" : "");
                case "float":
                    return "double" + (nullable ? "?" : "");
                case "int":
                    return "int" + (nullable ? "?" : "");
                case "nchar":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return "string";
                case "real":
                    return "single" + (nullable ? "?" : "");
                case "smallint":
                    return "short" + (nullable ? "?" : "");
                case "tinyint":
                    return "byte" + (nullable ? "?" : "");
                case "uniqueidentifier":
                    return "System.Guid" + (nullable ? "?" : "");
                case "date":
                    return "System.DateTime" + (nullable ? "?" : "");
                case "rowversion":
                    return "byte[]";

                default:
                    return "object";
            }
        }
    }
}
