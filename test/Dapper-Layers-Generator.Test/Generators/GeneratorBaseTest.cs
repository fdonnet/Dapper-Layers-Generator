using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper_Layers_Generator.Test.Config;
using System.Text.Json;
using Dapper_Layers_Generator.Data.POCO;

namespace Dapper_Layers_Generator.Test.Generators
{
    public class GeneratorBaseTest
    {
        protected Mock<IReaderDBDefinitionService> _mockDbDefinitions;
        protected SettingsGlobal _settings;

        public GeneratorBaseTest()
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
    }
}

