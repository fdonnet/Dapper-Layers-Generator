namespace Dapper_Layers_Generator.Data.POCO
{
    public interface ISchema
    {
        public string Name { get; set; }
        public IList<ITable>? Tables { get; set; }
    }

}
