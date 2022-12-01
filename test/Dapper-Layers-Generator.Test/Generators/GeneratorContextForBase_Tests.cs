using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Test.Config;
using Moq;
using System.Text.Json;
using Dapper_Layers_Generator.Data.POCO;
using Dapper_Layers_Generator.Core.Converters;

namespace Dapper_Layers_Generator.Core.Generators.Tests
{
    public class GeneratorContextForBase_Tests
    {

        private Mock<IReaderDBDefinitionService> _mockDbDefinitions;
        private SettingsGlobal _settings;

        public GeneratorContextForBase_Tests()
        {
            //Fake DB
            var jsonDB = ResourceTool.Read("Dapper_Layers_Generator.Test/Data/schema.json");
            var schemas = JsonSerializer.Deserialize<List<Schema>>(jsonDB) ?? throw new NullReferenceException("Cannot test without JSON DB");
            _mockDbDefinitions = new Mock<IReaderDBDefinitionService>();
            _mockDbDefinitions.Setup(x => x.SchemaDefinitions).Returns(schemas);

            //Standard settings
            var settings = ResourceTool.Read("Dapper_Layers_Generator.Test/Data/baseconfig.json");
            _settings = JsonSerializer.Deserialize<SettingsGlobal>(settings) ?? throw new NullReferenceException("Cannot test without standard settings");
        }

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