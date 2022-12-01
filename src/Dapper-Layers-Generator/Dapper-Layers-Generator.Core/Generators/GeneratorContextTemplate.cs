using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;

namespace Dapper_Layers_Generator.Core.Generators
{
    /// <summary>
    /// Abstract that define the minimum for DbContextBase and DBContext (generator)
    /// DbCOntext will have specific implementations depending of the db provider.
    /// </summary>
    public abstract class GeneratorContextTemplate : Generator
    {
        protected IEnumerable<Table>? _selectedTables;
        public GeneratorContextTemplate(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService)
            : base(settingsGlobal, data, stringTransformationService)
        {
            if (_currentSchema.Tables == null)
                throw new NullReferenceException("Cannot generate without at least one table selected");
            _selectedTables = _currentSchema.Tables.Where(t => GetSelectedTableNames().Contains(t.Name));
        }

        public override abstract string Generate();
        protected abstract string WriteContextHeaderComment();
        protected abstract string WriteUsingStatements();
        protected abstract string WriteFullClassContent();
        protected abstract string WriteClassRepoMembers();
    }
}
