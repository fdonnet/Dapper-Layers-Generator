namespace Dapper_Layers_Generator.Data.POCO.MySql
{
    public class MySqlTable :ITable
    {
        public string Schema { get; set; } = null!;
        public string Name { get; set; } = null!;
        public IList<IColumn>? Columns { get; set; }

    }
}
