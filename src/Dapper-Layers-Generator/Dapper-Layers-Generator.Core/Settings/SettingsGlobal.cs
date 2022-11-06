using System.Text.Json;

namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsGlobal
    {
        //Settings strings (custom attribute for reflection)
        [SettingsAttribute(Message = "For witch DBs you want to generate the dapper layers (multiple is possible): ", Position = 1)]
        public string TargetDbProviderForGeneration { get; set; } = "MySql";

        [SettingsAttribute(Message ="Selected schema: ", Position =2)]
        public string SelectedSchema { get; set; } = string.Empty;

        [SettingsAttribute(Message = "Auhtor name (used in code comments): ", Position = 3)]
        public string AuthorName { get; set; } = "Dapper Layers Generator User";

        [SettingsAttribute(Message = "Target project namespace: ", Position = 4)]
        public string TargetProjectNamespace { get; set; } = "MyTestProject";

        [SettingsAttribute(Message = "---  Target namespace for POCO (parent namespace is Project): ", Position = 5, ChildOf = "TargetProjectNamespace")]
        public string TargetNamespaceForPOCO { get; set; } = "MyTestProject.POCO";

        [SettingsAttribute(Message = "---  Target namespace for Repo (parent namespace is Project): ", Position = 6, ChildOf = "TargetProjectNamespace")]
        public string TargetNamespaceForRepo { get; set; } = "MyTestProject.Repo";

        [SettingsAttribute(Message = "---  Target namespace for DbContext (parent namespace is Project): ", Position = 7,ChildOf = "TargetProjectNamespace")]
        public string TargetNamespaceForDbContext { get; set; } = "MyTestProject.DbContext";

        [SettingsAttribute(Message = "Target project path: ", Position = 8)]
        public string TargetProjectPath { get; set; } = @"c:\temp\MyTestProject\";

        [SettingsAttribute(Message = "---  Target folder for POCO files: ", Position = 9, ChildOf = "TargetProjectPath")]
        public string TargetFolderForPOCO { get; set; } = @"c:\temp\MyTestProject\POCO\Generated\";

        [SettingsAttribute(Message = "---  Target folder for repo files: ", Position = 10, ChildOf = "TargetProjectPath")]
        public string TargetFolderForRepo { get; set; } = @"c:\temp\MyTestProject\Repo\Generated\";

        [SettingsAttribute(Message = "--- Target folder for DB context file: ", Position = 11, ChildOf = "TargetProjectPath")]
        public string TargetFolderForDBContext { get; set; } = @"c:\temp\MyTestProject\DbContext\Generated\";

        [SettingsAttribute(Message = "Connection string name for configuration injection (ex Default): ", Position = 12)]
        public string ConnectionStringName { get; set; } = "Default";
        [SettingsAttribute(Message = "Db context class name: ", Position = 13)]
        public string DbContextClassName { get; set; } = "DbContext";

        //Will be set based on db convention (mariadb = true, postgresql = true, mssql=false etc)
        [SettingsAttribute(Message = "Enable PascalCase transform for all table & column names: ", Position = 14)]
        public bool UsePascalTransform { get; set; } = true;

        //Will be set based on db convention (need to be tested)
        [SettingsAttribute(Message = "Enable Singularize transform for all table names: ", Position = 15)]
        public bool UseSingularizeTransform { get; set; } = true;

        [SettingsAttribute(Message = "What's your indent way for generated code: ", Position = 16)]
        public string IndentStringInGeneratedCode { get; set; } = "tab";

        //Tables selection to be generated (all or list of table names)
        public bool RunGeneratorForAllTables { get; set; } = true;
        public List<string> RunGeneratorForSelectedTables { get; set; } = new List<string>();

        //Global table settings 
        public SettingsTable TableGlobalSettings { get; set; } = new SettingsTable();

        //Override table global seetings (table_name key / table_seetings)
        public Dictionary<string, SettingsTable> TableSettings { get; set; } = new Dictionary<string, SettingsTable>();

        public async Task SaveToFile(string configPath)
        {
            using FileStream createStream = File.Create(configPath);
            await JsonSerializer.SerializeAsync(createStream, this, new JsonSerializerOptions() { WriteIndented = true });
            await createStream.DisposeAsync();
        }

        public async Task LoadFromFile(string configPath)
        {
            using FileStream openStream = File.OpenRead(configPath);

            var mySettings = openStream == null ? null : await JsonSerializer.DeserializeAsync<SettingsGlobal>(openStream);

            if (mySettings != null)
            {
                AuthorName = mySettings.AuthorName;
                SelectedSchema = mySettings.SelectedSchema;
                TargetProjectNamespace = mySettings.TargetProjectNamespace;
                TargetNamespaceForRepo = mySettings.TargetNamespaceForRepo; 
                TargetNamespaceForPOCO = mySettings.TargetNamespaceForPOCO;
                TargetNamespaceForDbContext = mySettings.TargetNamespaceForDbContext;
                TargetFolderForDBContext = mySettings.TargetFolderForDBContext;
                TargetFolderForPOCO = mySettings.TargetFolderForPOCO;
                TargetFolderForRepo = mySettings.TargetFolderForRepo;
                TargetProjectPath = mySettings.TargetProjectPath;
                ConnectionStringName = mySettings.ConnectionStringName;
                DbContextClassName = mySettings.DbContextClassName;
                UsePascalTransform = mySettings.UsePascalTransform;
                UseSingularizeTransform = mySettings.UseSingularizeTransform;
                IndentStringInGeneratedCode = mySettings.IndentStringInGeneratedCode;
                TargetDbProviderForGeneration = mySettings.TargetDbProviderForGeneration;
                RunGeneratorForAllTables = mySettings.RunGeneratorForAllTables;
                RunGeneratorForSelectedTables = mySettings.RunGeneratorForSelectedTables;
                TableGlobalSettings = mySettings.TableGlobalSettings;
                TableSettings = mySettings.TableSettings;
            }
        }

        public SettingsTable GetTableSettings(string tableName)
        {
            return TableSettings.TryGetValue(tableName, out var tabSettings) ? tabSettings : TableGlobalSettings;
        }
    }
}
