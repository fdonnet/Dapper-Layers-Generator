using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGenerator
    {
        Task<string> Generate();
    }

    public abstract class Generator : IGenerator
    {
        private readonly SettingsGlobal _settings;

        public Generator(SettingsGlobal settingsGlobal)
        {
            _settings = settingsGlobal;
        }

        public abstract Task<string> Generate();
    }
}
