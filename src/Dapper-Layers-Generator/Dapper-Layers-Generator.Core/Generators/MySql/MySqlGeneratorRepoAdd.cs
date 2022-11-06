using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public class MySqlGeneratorRepoAdd : GeneratorRepoAdd, IGeneratorRepoAdd
    {
        public MySqlGeneratorRepoAdd(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter) 
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }
    }
}
