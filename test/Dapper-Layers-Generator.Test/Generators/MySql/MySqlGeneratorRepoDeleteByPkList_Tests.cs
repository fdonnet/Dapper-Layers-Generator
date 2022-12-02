using Dapper_Layers_Generator.Core.Converters.MySql;
using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Test.Config;
using Dapper_Layers_Generator.Core.Generators.MySql;

namespace Dapper_Layers_Generator.Test.Generators.MySql
{
    public class MySqlGeneratorRepoDeleteByPkList_Tests : GeneratorBaseTest
    {
        private MySqlDataTypeConverter _dataConverter;
        private StringTransformationService _transformString;

        public MySqlGeneratorRepoDeleteByPkList_Tests() : base()
        {
            _dataConverter = new MySqlDataTypeConverter();
            _transformString = new StringTransformationService(_settings);
        }

        [Fact()]
        public void GenerateStandard_Test()
        {
            //Arrange
            var generator = new MySqlGeneratorRepoDeleteByPkList(_settings, _mockDbDefinitions.Object, _transformString, _dataConverter);
            generator.SetTable("clients");
            var expected = ResourceTool.Read("Dapper_Layers_Generator.Test/Results/Generators/MySql/MySqlGeneratorRepoDeleteByPkList_ResultStandard.txt");

            //Act
            var result = generator.Generate();

            //Assert
            Assert.Equal(expected, result);
        }

        [Fact()]
        public void GenerateCompositePk_Test()
        {
            //Arrange
            var generator = new MySqlGeneratorRepoDeleteByPkList(_settings, _mockDbDefinitions.Object, _transformString, _dataConverter);
            generator.SetTable("damages_failures");
            var expected = ResourceTool.Read("Dapper_Layers_Generator.Test/Results/Generators/MySql/MySqlGeneratorRepoDeleteByPkList_ResultCompositePk.txt");

            //Act
            var result = generator.Generate();

            //Assert
            Assert.Equal(expected, result);
        }
    }
}
