using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using Dapper_Layers_Generator.Data.Reader;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core
{
    public interface IGeneratorService
    {
        SettingsGlobal GlobalGeneratorSettings { get; set; }
    }

    public class GeneratorService : IGeneratorService
    {
        public SettingsGlobal GlobalGeneratorSettings { get; set; } 

        public GeneratorService(SettingsGlobal settingsGlobal)
        {
            GlobalGeneratorSettings = settingsGlobal;
        }


    }
}
