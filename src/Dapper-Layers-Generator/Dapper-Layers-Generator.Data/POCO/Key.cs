namespace Dapper_Layers_Generator.Data.POCO
{
    public class Key
    {
        public string Schema { get; set; } = null!;
        public string Table { get; set; } = null!;
        public string Column { get; set; } = null!;
        public string Name { get; set; } = null!;
        public KeyType Type { get; set; }

    }

    public enum KeyType
    {
        Primary,
        Unique
    }
}
