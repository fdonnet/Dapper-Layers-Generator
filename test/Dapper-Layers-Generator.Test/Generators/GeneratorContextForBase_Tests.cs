using Dapper_Layers_Generator.Test.Config;
using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Test.Generators;

namespace Dapper_Layers_Generator.Core.Generators.Tests
{
    public class GeneratorContextForBase_Tests : GeneratorBaseTest
    {
        [Fact()]
        public void GenerateStandard_Test()
        {
            //Arrange
            var transformString = new StringTransformationService(_settings);
            var generator = new GeneratorContextForBase(_settings, _mockDbDefinitions.Object, transformString);
            var expected = ResourceTool.Read("Dapper_Layers_Generator.Test/Results/Generators/GeneratorContextForBase_ResultStandard.txt");

            //Act
            var result = generator.Generate();

            //Assert
            Assert.Equal(expected, result);
        }

        [Fact()]
        public void GenerateChangeDbContextName_Test()
        {
            //Arrange
            var transformString = new StringTransformationService(_settings);
            var generator = new GeneratorContextForBase(_settings, _mockDbDefinitions.Object, transformString);
            _settings.DbContextClassName = "TestOtherDBContext";

            var expected = ResourceTool.Read("Dapper_Layers_Generator.Test/Results/Generators/GeneratorContextForBase_ResultOtherDbContextName.txt");

            //Act
            var result = generator.Generate();

            //Assert
            Assert.Equal(expected, result);
        }



    }
}