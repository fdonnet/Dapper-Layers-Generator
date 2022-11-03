using Dapper_Layers_Generator.Core.Settings;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorsProvider
    {
        T GetGenerator<T>();
        IEnumerable<IGenerator> GetSelectedGeneratorsForRepo(string tableName);
    }
    public class GeneratorsProvider : IGeneratorsProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SettingsGlobal _settings;

        public GeneratorsProvider(IServiceProvider serviceProvider, SettingsGlobal settings)
        {
            _serviceProvider = serviceProvider;
            _settings = settings;
        }

        public IEnumerable<IGenerator> GetSelectedGeneratorsForRepo(string tableName)
        {
            var generators = new List<IGenerator>();

            SettingsTable curSettings = _settings.TableSettings.TryGetValue(tableName, out var tabSettings) 
                ? tabSettings 
                : _settings.TableGlobalSettings;

            if(curSettings.AddGenerator)
            {
                generators.Add((IGenerator)_serviceProvider.GetService(typeof(IGeneratorRepoAdd))!);
            }

            return generators;
        }

        public T GetGenerator<T>()
        {
            return (T)_serviceProvider.GetService(typeof(T))!;
        }

    }
}
