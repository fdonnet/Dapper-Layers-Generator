namespace Dapper_Layers_Generator.Data.POCO.MySql
{
    public class MySqlSchema : ISchema
    {
        public string Name { get; set; } = null!;
        public IList<ITable>? Tables { get; set; }
    }

}

