using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoAdd
    {

    }
    public class GeneratorRepoAdd : GeneratorFromTable, IGeneratorRepoAdd
    {
        public GeneratorRepoAdd(SettingsGlobal settingsGlobal, IReaderDBDefinitionService data) : base(settingsGlobal, data)
        {

        }

        public override string Generate()
        {
            return "TEST";
        }
    }
}
