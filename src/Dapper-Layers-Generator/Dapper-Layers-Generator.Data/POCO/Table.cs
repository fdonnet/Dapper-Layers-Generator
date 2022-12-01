namespace Dapper_Layers_Generator.Data.POCO
{
    public class Table
    {
        public string Schema { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<Column>? Columns { get; set; }
    }

}
