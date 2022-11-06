using Dapper_Layers_Generator.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorsProvider
    {
        T GetGenerator<T>(IServiceScope scope);
        T GetGenerator<T>(string tableName, IServiceScope scope);
        IEnumerable<IGeneratorFromTable> GetSelectedGeneratorsForRepo(string tableName, IServiceScope scope);
    }
    public class GeneratorsProvider : IGeneratorsProvider
    {
        private readonly SettingsGlobal _settings;

        public GeneratorsProvider(SettingsGlobal settings)
        {
            _settings = settings;
        }

        public IEnumerable<IGeneratorFromTable> GetSelectedGeneratorsForRepo(string tableName, IServiceScope scope)
        {
            var generators = new List<IGeneratorFromTable>();

            SettingsTable curSettings = _settings.TableSettings.TryGetValue(tableName, out var tabSettings) 
                ? tabSettings 
                : _settings.TableGlobalSettings;

            if(curSettings.AddGenerator)
            {
                generators.Add((IGeneratorFromTable)GetGenerator<IGeneratorRepoAdd>(tableName,scope));
            }

            return generators;
        }

       //Get a real scoped generator
        public T GetGenerator<T>(IServiceScope scope)
        {
            return (T)scope.ServiceProvider.GetService(typeof(T))!;
        }

        public T GetGenerator<T>(string tableName, IServiceScope scope)
        {
            var generator = (IGeneratorFromTable)scope.ServiceProvider.GetService(typeof(T))!;
            generator.SetTable(tableName);

            return (T)generator;
        }

    }
}
