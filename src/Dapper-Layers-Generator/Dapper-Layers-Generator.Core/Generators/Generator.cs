using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System.Reflection.Metadata.Ecma335;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGenerator
    {
        string Generate();
        List<string> GetSelectedTableNames();
    }

    public abstract class Generator : IGenerator
    {
        protected readonly SettingsGlobal _settings;
        protected readonly ISchema _currentSchema;
        protected readonly StringTransformationService _stringTransform;
        private readonly IReaderDBDefinitionService _data;
#pragma warning disable IDE1006 // Naming Styles
        protected string tab { get { return _stringTransform.IndentString; } }
#pragma warning restore IDE1006 // Naming Styles

        public Generator(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService)
        {
            _settings = settingsGlobal;
            _data = data;

            var schema = _data.SchemaDefinitions?.Where(s => s.Name == _settings.SelectedSchema).SingleOrDefault();
            if (schema == null)
                throw new NullReferenceException("Selected DB schema cannot be null in the config when you want to use a generator");

            _currentSchema = schema;

            _stringTransform = stringTransformationService;
        }

        public List<string> GetSelectedTableNames()
        {
            var selectedTableNames = new List<string>();
            if (_settings.RunGeneratorForAllTables)
            {
                selectedTableNames = _data.SchemaDefinitions?.Where(s => s.Name == _settings.SelectedSchema)
                                                                .SingleOrDefault()?.Tables?.ToList().Select(t => t.Name).ToList();

                if (selectedTableNames == null || selectedTableNames.Count == 0)
                {
                    throw new NullReferenceException("No db defintions found to generate anything...");
                }
            }
            else
            {
                selectedTableNames = _settings.RunGeneratorForSelectedTables;
            }
            return selectedTableNames;
        }

        public abstract string Generate();

    }
}
