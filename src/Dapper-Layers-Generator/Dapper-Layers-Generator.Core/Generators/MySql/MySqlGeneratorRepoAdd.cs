using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoAdd : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoAdd : GeneratorRepoAdd, IMySqlGeneratorRepoAdd
    {
        public MySqlGeneratorRepoAdd(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
    {
        ColAndTableIdentifier = "`";
        IsBase = false;
    }

        protected override string WriteValuesToInsert()
        {

            var cols = ColumnForInsertOperations!.Where(c => !c.IsAutoIncrement);
            var values = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},", cols.OrderBy(c => c.Position).Select(col =>
            {
                return $@"@{col.Name}";
            }));

            string identityClause = string.Empty;

            if(PkColumns.Count() == 1 && PkColumns.Where(c=>c.IsAutoIncrement).Any())
            {
                identityClause =
                    $""""
                    {tab}{tab}{tab});
                    {tab}{tab}{tab}SELECT LAST_INSERT_ID();
                    {tab}{tab}{tab}""";
                    """";
            }
            else
                identityClause =
                    $""""
                    {tab}{tab}{tab})
                    {tab}{tab}{tab}""";
                    """";
            return
                $"""
                {tab}{tab}{tab}{tab}{values}
                {identityClause}
                """;
        }
    }
}
