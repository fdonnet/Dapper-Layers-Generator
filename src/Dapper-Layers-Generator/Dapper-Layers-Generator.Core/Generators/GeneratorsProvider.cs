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
        IEnumerable<IGenerator> GetGeneratorsForRepo(string tableName);
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

        public IEnumerable<IGenerator> GetGeneratorsForRepo(string tableName)
        {
            var generators = new List<IGenerator>();

            var service = _serviceProvider.GetService(typeof(IGeneratorRepoAdd));
            generators.Add((IGenerator)service!);

            return generators;
        }

        public IGeneratorPOCO GetGeneratorForContext()
        {
            var generator = (IGeneratorPOCO)_serviceProvider.GetService(typeof(IGeneratorPOCO))!;
            
            return generator;

        }

        public IGeneratorPOCO GetGeneratorForPOCO()
        {
            var generator = (IGeneratorPOCO)_serviceProvider.GetService(typeof(IGeneratorPOCO))!;

            return generator;
        }

    }
}
