using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoAdd
    {

    }
    public class GeneratorRepoAdd : GeneratorFromTable, IGeneratorRepoAdd
    {
        protected virtual string ColAndTableIdentifier { get; init; } = String.Empty;

        public GeneratorRepoAdd(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter) 
                : base(settingsGlobal, data, stringTransformationService,dataConverter)
        {

        }

        public override string Generate()
        {
            return "";
        }
    }
}
