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
        public List<string>? UniqueIndexNames { get; set; }
        public bool IsAutoIncrement { get; set; }
        public string CompleteType { get; set; } = null!;
    }
}
