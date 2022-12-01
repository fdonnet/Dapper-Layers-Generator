using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoUpdate : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoUpdate : GeneratorRepoUpdate, IMySqlGeneratorRepoUpdate
    {
        public MySqlGeneratorRepoUpdate(SettingsGlobal settingsGlobal
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
