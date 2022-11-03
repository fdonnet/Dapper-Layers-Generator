﻿using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGenerator
    {
       string Generate();
    }

    public abstract class Generator : IGenerator
    {
        protected readonly SettingsGlobal _settings;
        protected readonly ISchema _currentSchema;
        protected readonly StringTransformationService _stringTransform;

        private readonly IReaderDBDefinitionService _data;

        public Generator(SettingsGlobal settingsGlobal, IReaderDBDefinitionService data, StringTransformationService stringTransformationService)
        {
            _settings = settingsGlobal;
            _data = data;

            var schema = _data.SchemaDefinitions?.Where(s => s.Name == _settings.SelectedSchema).SingleOrDefault();
            if (schema == null)
                throw new NullReferenceException("Selected DB schema cannot be null in the config when you want to use a generator");

            _currentSchema = schema;

            _stringTransform = stringTransformationService;
        }

        public abstract string Generate();

    }
}
