using Xunit;
using Dapper_Layers_Generator.Core.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper_Layers_Generator.Test.Generators;
using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Test.Config;
using Dapper_Layers_Generator.Core.Converters.MySql;
using Moq;
using System.Text.Json;
using Dapper_Layers_Generator.Data.POCO;

namespace Dapper_Layers_Generator.Core.Generators.Tests
{
    public class GeneratorPOCO_Tests : GeneratorBaseTest
    {

        [Fact()]
        public void SetTableNotFound_Test()
        {
            //Arrange
            var transformString = new StringTransformationService(_settings);
            //need to be reviewed if dataconverted will change between db provider
            var dataConverter = new MySqlDataTypeConverter();
            var generator = new GeneratorPOCO(_settings, _mockDbDefinitions.Object, transformString, dataConverter);

            //Act
            var exception = Record.Exception(() => generator.SetTable("zzzz"));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<NullReferenceException>(exception);
        }


        [Fact()]
        public void SetTableColumnsNotFound_Test()
        {
            //Arrange
            var transformString = new StringTransformationService(_settings);
            //need to be reviewed if dataconverted will change between db provider
            var dataConverter = new MySqlDataTypeConverter();
            var jsonDB = ResourceTool.Read("Dapper_Layers_Generator.Test/Data/schema.json");
            var schemas = JsonSerializer.Deserialize<List<Schema>>(jsonDB) ?? throw new NullReferenceException("Cannot test without JSON DB");
            schemas[0].Tables!.Where(t => t.Name == "clients").Single().Columns =null;
            var mockDbDefinitions = new Mock<IReaderDBDefinitionService>();
            mockDbDefinitions.Setup(x => x.SchemaDefinitions).Returns(schemas);

            var generator = new GeneratorPOCO(_settings, mockDbDefinitions.Object, transformString, dataConverter);

            //Act
            var exception = Record.Exception(() => generator.SetTable("clients"));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact()]
        public void Generate_Test()
        {

        }
    }
}