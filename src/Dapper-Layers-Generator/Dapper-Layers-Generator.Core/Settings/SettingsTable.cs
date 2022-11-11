namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsTable
    {
        [SettingsAttribute(Message = "Ignored column names for POCO and all operations (separator ,): ", Position = 1, IsColumnListChoice = true)]
        public string IgnoredColumnNames { get; set; } = string.Empty;

        [SettingsAttribute(Message = "Ignored column names for Get operation (separator ,): ", Position = 2, IsColumnListChoice = true)]
        public string IgnoredColumnNamesForGet { get; set; } = string.Empty;

        [SettingsAttribute(Message = "Ignored column names for Add operation (separator ,): ", Position = 3, IsColumnListChoice = true)]
        public string IgnoredColumnNamesForAdd { get; set; } = string.Empty;

        [SettingsAttribute(Message = "Ignored column names for Update operation (separator ,): ", Position = 4, IsColumnListChoice = true)]
        public string IgnoredColumnNamesForUpdate { get; set; } = string.Empty;

        [SettingsAttribute(Message = "JSON ignore decoration for column names  (separator ,): ", Position = 5, IsColumnListChoice = true)]
        public string JsonIgnoreDecoration { get; set; } = string.Empty;

        [SettingsAttribute(Message = "Enable get all function generator: ", Position = 6, Group = "Generator")]
        public bool GetAllGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable get by primary key function generator: ", Position = 7, Group = "Generator")]
        public bool GetByPkGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable get by primary keys list function generator: ", Position = 8, Group = "Generator")]
        public bool GetByPkListGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable get by unique key function generator: ", Position = 9, Group = "Generator")]
        public bool GetByUkGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable add function generator: ", Position = 10, Group = "Generator")]
        public bool AddGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable add function generator (multi-insert): ", Position = 11, Group = "Generator")]
        public bool AddMultiGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable add bulk function generator: ", Position = 12, Group = "Generator")]
        public bool AddBulkGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable update function generator: ", Position = 13, Group = "Generator")]
        public bool UpdateGenerator { get; set; } = true;

        [SettingsAttribute(Message = "Enable delete function generator: ", Position = 14, Group = "Generator")]
        public bool DeleteGenerator { get; set; } = true;

        [SettingsAttribute(Message = "(NOT IMPLEMENTED) Enable update bulk function generator: ", Position = 15, Group = "Generator")]
        public bool UpdateBulkGenerator { get; set; } = false;

        //Global table settings 
        public SettingsColumn ColumnGlobalSettings { get; set; } = new SettingsColumn();

        //Override table global seetings
        public Dictionary<string, SettingsColumn> ColumnSettings { get; set; } = new Dictionary<string, SettingsColumn>();

        public SettingsColumn GetColumnSettings(string colName)
        {
            if (ColumnSettings.TryGetValue(colName, out var colSettings))
            {
                return colSettings;
            }
            else
                return ColumnGlobalSettings;
        }

    }
}
