namespace Dapper_Layers_Generator.Data.POCO
{
    public class Schema
    {
        public string Name { get; set; } = null!;
        public List<Table>? Tables { get; set; }
    }

}
