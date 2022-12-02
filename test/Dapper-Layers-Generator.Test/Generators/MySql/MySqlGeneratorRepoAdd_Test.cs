using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Converters.MySql;
using Dapper_Layers_Generator.Core.Generators.MySql;
using Dapper_Layers_Generator.Test.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Test.Generators.MySql
{
    public class MySqlGeneratorRepoAdd_Test : GeneratorBaseTest
    {
        //need to be reviewed if dataconverted will change between db provider
        private MySqlDataTypeConverter _dataConverter;
        private StringTransformationService _transformString;

        public MySqlGeneratorRepoAdd_Test() : base()
        {
            _dataConverter = new MySqlDataTypeConverter();
            _transformString = new StringTransformationService(_settings);
        }

        [Fact()]
        public void GenerateStandard_Test()
        {
            //Arrange
            var generator = new MySqlGeneratorRepoAdd(_settings, _mockDbDefinitions.Object, _transformString, _dataConverter);
            generator.SetTable("clients");
            var expected = ResourceTool.Read("Dapper_Layers_Generator.Test/Results/Generators/MySql/MySqlGeneratorRepoAdd_ResultStandard.txt");

            //Act
            var result = generator.Generate();

            //Assert
            Assert.Equal(expected, result);
        }
    }
}
