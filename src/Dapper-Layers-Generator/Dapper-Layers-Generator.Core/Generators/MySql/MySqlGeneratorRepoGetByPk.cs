using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoGetByPk : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoGetByPk : GeneratorRepoGetByPk, IMySqlGeneratorRepoGetByPk
    {
        public MySqlGeneratorRepoGetByPk(SettingsGlobal settingsGlobal
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
