using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsTable
    {
        //Can ignore certain columns names (separated by ,)
        [SettingsAttribute(Message = "Ignored column names in all tables (separator ,): ", Position = 1)]
        public string IgnoredColumnNames { get; set; } = string.Empty;

        //Global table settings 
        public SettingsColumn ColumnGlobalSettings { get; set; } = new SettingsColumn();

        //Override table global seetings
        public Dictionary<string, SettingsColumn> ColumnSettings { get; set; } = new Dictionary<string, SettingsColumn>();
    }
}
