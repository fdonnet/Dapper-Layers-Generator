using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoGetAll : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoGetAll : GeneratorRepoGetAll, IMySqlGeneratorRepoGetAll
    {
        public MySqlGeneratorRepoGetAll(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {
            ColAndTableIdentifier = "`";
            IsBase = false;
        }


    }
}
