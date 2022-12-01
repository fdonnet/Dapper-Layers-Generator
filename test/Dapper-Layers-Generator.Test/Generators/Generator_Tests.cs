﻿using Xunit;
using Dapper_Layers_Generator.Core.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Dapper_Layers_Generator.Data.Reader;
using Dapper_Layers_Generator.Data.POCO;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Test.Config;
using Dapper_Layers_Generator.Core.Generators.MySql;
using System.Reflection.Emit;


namespace Dapper_Layers_Generator.Core.Generators.Tests
{
    public class Generator_Tests
    {
        private Mock<IReaderDBDefinitionService> _mockDbDefinitions;
        private SettingsGlobal _settings = new();
        
        public Generator_Tests()
        {
            var jsonDB = JsonDB.Read("Dapper_Layers_Generator.Test/Data/schema.json");
            var schemas = JsonSerializer.Deserialize<List<Schema>>(jsonDB) ?? throw new NullReferenceException("Cannot test without JSON DB");
            _mockDbDefinitions = new Mock<IReaderDBDefinitionService>();
            _mockDbDefinitions.Setup(x => x.SchemaDefinitions).Returns(schemas);
            _settings.SelectedSchema = "mobileflow";
        }

        [Fact]
        public void GetSelectedTableNamesWithSelection_Test()
        {
            //Arrange
            _settings.RunGeneratorForAllTables = false;
            var tableSelection = new List<string>() { "clients", "failures" };
            _settings.RunGeneratorForSelectedTables = tableSelection;
            var transformString = new StringTransformationService(_settings);
            var generator = new MySqlGeneratorContext(_settings, _mockDbDefinitions.Object, transformString);

            //Act
            var result = generator.GetSelectedTableNames();

            //Assert
            Assert.Equal(tableSelection.OrderBy(t => t), result.OrderBy(t => t));
        }

        [Fact]
        public void GetSelectedTableNamesAll_Test()
        {
            //Arrange
            _settings.RunGeneratorForAllTables = true;
            var transformString = new StringTransformationService(_settings);
            var generator = new MySqlGeneratorContext(_settings, _mockDbDefinitions.Object, transformString);
            var expectedTables = new List<string>() { "states", "clients", "refresh_token", "historystates", "repairers", "users", "damages", "damages_failures", "failures" };

            //Act
            var result = generator.GetSelectedTableNames();

            //Assert
            Assert.Equal(expectedTables.OrderBy(t => t), result.OrderBy(t => t));
        }

        [Fact]
        public void GetSelectedTableNamesTablesNull_Test()
        {
            //Arrange
            _settings.RunGeneratorForAllTables = true;
            List<Schema>? nullSchema = null;
            var transformString = new StringTransformationService(_settings);
            var generator = new MySqlGeneratorContext(_settings, _mockDbDefinitions.Object, transformString);
            _mockDbDefinitions.Setup(x => x.SchemaDefinitions).Returns(nullSchema);

            //Act
            var exception = Record.Exception(() => generator.GetSelectedTableNames());

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<NullReferenceException>(exception);
        }

        [Fact]
        public void GetSelectedTableNamesTablesNotFound_Test()
        {
            //Arrange
            _settings.RunGeneratorForAllTables = true;
            List<Schema>? emptyTables = new() { new Schema() { Tables = new List<Table>(), Name = "mobileflow" } };
            var transformString = new StringTransformationService(_settings);
            var generator = new MySqlGeneratorContext(_settings, _mockDbDefinitions.Object, transformString);
            _mockDbDefinitions.Setup(x => x.SchemaDefinitions).Returns(emptyTables);

            //Act
            var exception = Record.Exception(() => generator.GetSelectedTableNames());

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<NullReferenceException>(exception);
        }
    }
}