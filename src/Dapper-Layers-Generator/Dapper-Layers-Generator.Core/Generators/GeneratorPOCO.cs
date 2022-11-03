using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorPOCO
    {

    }

    public class GeneratorPOCO : Generator , IGeneratorPOCO
    {
        public GeneratorPOCO(SettingsGlobal settingsGlobal) : base(settingsGlobal)
        {
            
        }

        public override async Task<string> Generate()
        {
            return "TEST";
        }
    }
}
