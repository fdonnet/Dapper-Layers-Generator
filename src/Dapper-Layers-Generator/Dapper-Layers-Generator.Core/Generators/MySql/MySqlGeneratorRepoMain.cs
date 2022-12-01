using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoMain : IGeneratorFromTable
    {
    }
    public class MySqlGeneratorRepoMain : GeneratorRepoMain, IMySqlGeneratorRepoMain
    {
        public MySqlGeneratorRepoMain(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter) : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

            UsingDbProviderSpecific = "using MySqlConnector;" + Environment.NewLine + "using System.Data;";
            DbProviderString = "MySql";
        }


    }
}
