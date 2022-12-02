using Dapper_Layers_Generator.Core.Converters.MySql;
using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Generators;
using Dapper_Layers_Generator.Test.Config;

namespace Dapper_Layers_Generator.Test.Generators
{
    public class GeneratorRepoUpdate_Tests : GeneratorBaseTest
    {
        private MySqlDataTypeConverter _dataConverter;
        private StringTransformationService _transformString;

        public GeneratorRepoUpdate_Tests() : base()
        {
            _dataConverter = new MySqlDataTypeConverter();
            _transformString = new StringTransformationService(_settings);
        }

        [Fact()]
        public void GenerateStandard_Test()
        {
            //Arrange
            var generator = new GeneratorRepoUpdate(_settings, _mockDbDefinitions.Object, _transformString, _dataConverter);
            generator.SetTable("clients");
            var expected = ResourceTool.Read("Dapper_Layers_Generator.Test/Results/Generators/GeneratorRepoUpdate_ResultStandard.txt");

            //Act
            var result = generator.Generate();

            //Assert
            Assert.Equal(expected, result);
        }
    }
}
