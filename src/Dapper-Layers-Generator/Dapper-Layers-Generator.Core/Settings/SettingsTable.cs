using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsTable
    {
        [SettingsAttribute(Message = "Ignored column names (separator ,): ", Position = 1)]
        public string IgnoredColumnNames { get; set; } = string.Empty;

        [SettingsAttribute(Message = "Enable get all function generator: ", Position = 2, Group = "Generator")]
        public bool GetAllGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable get by primary key function generator: ", Position = 3, Group = "Generator")]
        public bool GetByPkGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable get by primary keys list function generator: ", Position = 4, Group = "Generator")]
        public bool GetByPkListGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable get by unique key function generator: ", Position = 5, Group = "Generator")]
        public bool GetByUkGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable add function generator: ", Position = 6, Group ="Generator" )]
        public bool AddGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable update function generator: ", Position = 7, Group = "Generator")]
        public bool UpdateGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable delete function generator: ", Position = 8, Group = "Generator")]
        public bool DeleteGenerator { get; set; } = true;

        [SettingsAttribute(Message = "(NOT IMPLEMENTED) Enable add bulk function generator: ", Position = 9, Group = "Generator")]
        public bool AddBulkGenerator { get; set; } = false;

        [SettingsAttribute(Message = "(NOT IMPLEMENTED) Enable update bulk function generator: ", Position = 10, Group = "Generator")]
        public bool UpdateBulkGenerator { get; set; } = false;

        //Global table settings 
        public SettingsColumn ColumnGlobalSettings { get; set; } = new SettingsColumn();

        //Override table global seetings
        public Dictionary<string, SettingsColumn> ColumnSettings { get; set; } = new Dictionary<string, SettingsColumn>();
    }
}
