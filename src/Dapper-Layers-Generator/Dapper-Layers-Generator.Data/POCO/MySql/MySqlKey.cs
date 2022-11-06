namespace Dapper_Layers_Generator.Data.POCO.MySql
{
    public class MySqlKey : IKey
    {
        public string Schema { get; set; } = null!;
        public string Table { get; set; } = null!;
        public string Column { get; set; } = null!;
        public string Name { get; set; } = null!;
        public KeyType Type { get; set; }
    }
}
