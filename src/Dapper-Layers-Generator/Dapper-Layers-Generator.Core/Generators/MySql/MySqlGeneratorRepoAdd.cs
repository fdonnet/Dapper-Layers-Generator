using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public class MySqlGeneratorRepoAdd : GeneratorRepoAdd, IGeneratorRepoAdd
    {
        public MySqlGeneratorRepoAdd(SettingsGlobal settingsGlobal) : base(settingsGlobal)
        {

        }
    }
}
