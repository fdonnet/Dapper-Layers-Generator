namespace Dapper_Layers_Generator.Data.POCO
{
    public interface IKey
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
        public string Name { get; set; }
        public KeyType Type { get; set; }

    }

    public enum KeyType
    {
        Primary,
        Unique
    }
}
