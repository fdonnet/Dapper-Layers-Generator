using Dapper_Layers_Generator.Core.Converters.MySql;
using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Generators.MySql;
using Dapper_Layers_Generator.Test.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper_Layers_Generator.Core.Generators;

namespace Dapper_Layers_Generator.Test.Generators
{
    public class GeneratorRepoDeleteByPkList_Test : GeneratorBaseTest
    {
        private MySqlDataTypeConverter _dataConverter;
        private StringTransformationService _transformString;

        public GeneratorRepoDeleteByPkList_Test() : base()
        {
            _dataConverter = new MySqlDataTypeConverter();
            _transformString = new StringTransformationService(_settings);
        }

        [Fact()]
        public void GenerateStandard_Test()
        {
            //Arrange
            var generator = new GeneratorRepoDeleteByPkList(_settings, _mockDbDefinitions.Object, _transformString, _dataConverter);
            generator.SetTable("clients");
            var expected = ResourceTool.Read("Dapper_Layers_Generator.Test/Results/Generators/GeneratorRepoDeleteByPkList_ResultStandard.txt");

            //Act
            var result = generator.Generate();

            //Assert
            Assert.Equal(expected, result);
        }

        [Fact()]
        public void GenerateCompositePk_Test()
        {
            //Arrange
            var generator = new GeneratorRepoDeleteByPkList(_settings, _mockDbDefinitions.Object, _transformString, _dataConverter);
            generator.SetTable("damages_failures");
            var expected = ResourceTool.Read("Dapper_Layers_Generator.Test/Results/Generators/GeneratorRepoDeleteByPkList_ResultCompositePk.txt");

            //Act
            var result = generator.Generate();

            //Assert
            Assert.Equal(expected, result);
        }

    }
}
