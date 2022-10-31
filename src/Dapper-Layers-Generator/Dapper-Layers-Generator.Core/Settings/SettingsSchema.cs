using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsSchema :SettingsBase
    {
        public string OutputPath_ContextFile { get; set; } = "DapperLayersGenerator.Context.cs";
        public string Namespace_Context { get; set; } = "DapperLayersGenerator.Context";

        public bool RunGeneratorForAllTables { get; set; } = true;
        public IList<string> RunGeneratorForSelectedTables { get; set; } = new List<string>();

        //Global table settings 
        public SettingsTable TableGlobalSettings { get; set; } = new SettingsTable();

        //Override table global seetings
        public Dictionary<string, SettingsTable> TableSettings { get; set; } = new Dictionary<string, SettingsTable>();
    }
}
