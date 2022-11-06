using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorFromTable
    {   string ClassName { get; }
        void SetTable(string tableName);
    }

    public abstract class GeneratorFromTable : Generator, IGeneratorFromTable
    {
        //used by factory only can force null not set in constructor
        protected ITable Table { get; private set; } = null!;
        public string ClassName { get; private set; } = null!;
        protected SettingsTable TableSettings { get; private set; } = null!;
        protected IDataTypeConverter DataConverter { get; private set; } = null!;

        public GeneratorFromTable(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter) 
                : base(settingsGlobal, data, stringTransformationService)
        {
            DataConverter = dataConverter;
        }

        public override abstract string Generate();

        public void SetTable(string tableName)
        {
            var table = _currentSchema.Tables?.Where(t => t.Name == tableName).SingleOrDefault();

            if (table == null)
                throw new NullReferenceException("Table not found in the DB repository");

            Table = table;
            ClassName = _stringTransform.ApplyConfigTransform(Table.Name)!;
            TableSettings = _settings.GetTableSettings(Table.Name);
        }

    }
}
