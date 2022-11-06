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
                "tinyint" => "byte" + (nullable ? "?" : ""),
                "tinyint unsigned" => "byte" + (nullable ? "?" : ""),
                "uniqueidentifier" => "System.Guid" + (nullable ? "?" : ""),
                "date" => "DateTime" + (nullable ? "?" : ""),
                "datetime" or "smalldatetime" or "time" or "timestamp" => "DateTime" + (nullable ? "?" : ""),
                "rowversion" => "byte[]",
                _ => "object",
            };
        }
    }
}
