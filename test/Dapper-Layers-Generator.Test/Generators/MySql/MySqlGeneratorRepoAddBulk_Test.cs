using Dapper_Layers_Generator.Core.Converters.MySql;
using Dapper_Layers_Generator.Core.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper_Layers_Generator.Core.Generators.MySql;
using Dapper_Layers_Generator.Test.Config;

namespace Dapper_Layers_Generator.Test.Generators.MySql
{
    public class MySqlGeneratorRepoAddBulk_Test : GeneratorBaseTest
    {
        //need to be reviewed if dataconverted will change between db provider
        private MySqlDataTypeConverter _dataConverter;
        private StringTransformationService _transformString;

        public MySqlGeneratorRepoAddBulk_Test() : base()
        {
            _dataConverter = new MySqlDataTypeConverter();
            _transformString = new StringTransformationService(_settings);
        }

        [Fact()]
        public void GenerateStandard_Test()
        {
            //Arrange
            var generator = new MySqlGeneratorRepoAddBulk(_settings, _mockDbDefinitions.Object, _transformString, _dataConverter);
            generator.SetTable("clients");
            var expected = ResourceTool.Read("Dapper_Layers_Generator.Test/Results/Generators/MySql/MySqlGeneratorRepoAddBulk_ResultStandard.txt");

            //Act
            var result = generator.Generate();

            //Assert
            Assert.Equal(expected, result);
        }

    }
}
