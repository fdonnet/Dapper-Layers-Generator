using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsTable :SettingsBase
    {
        public bool GeneratePOCO { get; set; } = true;
        public bool ImplementInterfacePOCO { get; set; } = true;
        public bool GenerateRepo { get; set; } = true;

        //Global table settings 
        public SettingsColumn ColumnGlobalSettings { get; set; } = new SettingsColumn();

        //Override table global seetings
        public Dictionary<string, SettingsColumn> ColumnSettings { get; set; } = new Dictionary<string, SettingsColumn>();
    }
}
