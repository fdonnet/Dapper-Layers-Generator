namespace Dapper_Layers_Generator.Core.Converters
{
    public interface IDataTypeConverter
    {
        string GetDotNetDataType(string sqlDataTypeName, bool nullable = false);
        string GetSqlType(string sqlDataTypeName);
    }

    public abstract class DataTypeConverter
    {
        public virtual string GetDotNetDataType(string sqlDataTypeName, bool nullable = false)
        {
            return sqlDataTypeName == null
                ? throw new ArgumentNullException(nameof(sqlDataTypeName))
                : sqlDataTypeName.ToLower() switch
            {
                "bigint" => "long" + (nullable ? "?" : ""),
                "bigint unsigned" => "ulong" + (nullable ? "?" : ""),
                "binary" or "image" or "varbinary" => "byte[]" + (nullable ? "?" : ""),
                "bit" => "bool" + (nullable ? "?" : ""),
                "char" => "char" + (nullable ? "?" : ""),
                "decimal" or "money" or "numeric" => "decimal" + (nullable ? "?" : ""),
                "float" => "float" + (nullable ? "?" : ""),
                "int" or "medium int" => "int" + (nullable ? "?" : ""),
                "int unsigned" => "uint" + (nullable ? "?" : ""),
                "nchar" or "nvarchar" or "text" or "longtext" or "varchar" or "tinytext" or "xml" or "mediumtext" => "string" + (nullable ? "?" : ""),
                "real" => "single" + (nullable ? "?" : ""),
                "smallint" => "short" + (nullable ? "?" : ""),
                "smallint unsigned" => "ushort" + (nullable ? "?" : ""),
                "tinyint" => "bool" + (nullable ? "?" : ""), //VERY BAD for mySQL tinyint(1)
                "tinyint unsigned" => "byte" + (nullable ? "?" : ""), 
                "uniqueidentifier" => "System.Guid" + (nullable ? "?" : ""),
                "date" => "DateTime" + (nullable ? "?" : ""),
                "datetime" or "smalldatetime" or "time" or "timestamp" => "DateTime" + (nullable ? "?" : ""),
                "rowversion" => "byte[]",
                _ => "object",
            };
        }

        public virtual string GetSqlType(string sqlDataTypeName)
        {
            if (sqlDataTypeName == null) throw new ArgumentNullException(nameof(sqlDataTypeName));
            switch (sqlDataTypeName.ToLower())
            {
                case "bigint":
                    return "SqlInt64";
                case "binary":
                case "image":
                case "varbinary":
                    return "SqlBinary";
                case "bit":
                    return "SqlBoolean";
                case "char":
                    return "SqlString";
                case "datetime":
                case "smalldatetime":
                    return "SqlDateTime";
                case "decimal":
                case "money":
                case "numeric":
                    return "SqlDecimal";
                case "float":
                    return "SqlDouble";
                case "int":
                    return "SqlInt32";
                case "nchar":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return "SqlString";
                case "real":
                    return "SqlSingle";
                case "smallint":
                    return "SqlInt16";
                case "tinyint":
                    return "SqlBoolean";
                case "uniqueidentifier":
                    return "SqlGuid";
                case "date":
                    return "SqlDateTime";

                default:
                    return string.Empty;
            }
        }
    }
}
