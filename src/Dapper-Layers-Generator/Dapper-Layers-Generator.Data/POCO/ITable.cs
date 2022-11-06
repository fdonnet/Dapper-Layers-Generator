namespace Dapper_Layers_Generator.Data.POCO
{
    public interface ITable
    {
        public string Schema { get; set; }
        public string Name { get; set; } 
        public IList<IColumn>? Columns { get; set; }
    }

}
