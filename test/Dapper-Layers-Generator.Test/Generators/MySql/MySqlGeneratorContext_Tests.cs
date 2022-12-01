using Xunit;
using Dapper_Layers_Generator.Core.Generators.MySql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Test.Config;
using Moq;
using System.Text.Json;
using Dapper_Layers_Generator.Data.POCO;
using Dapper_Layers_Generator.Test.Generators;
using Dapper_Layers_Generator.Core.Converters;

namespace Dapper_Layers_Generator.Core.Generators.MySql.Tests
{
    public class MySqlGeneratorContext_Tests : GeneratorBaseTest
    {
        public MySqlGeneratorContext_Tests() : base()
        {

        }

        [Fact()]
        public void GenerateStandard_Test()
        {
            //Arrange
            var transformString = new StringTransformationService(_settings);
            var generator = new MySqlGeneratorContext(_settings, _mockDbDefinitions.Object, transformString);
            var expected = ResourceTool.Read("Dapper_Layers_Generator.Test/Results/Generators/MySql/MySqlGeneratorContext_ResultStandard.txt");

            //Act
            var result = generator.Generate();

            //Assert
            Assert.Equal(expected, result);
        }
    }
}