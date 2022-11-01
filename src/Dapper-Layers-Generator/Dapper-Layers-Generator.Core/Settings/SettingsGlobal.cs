using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsGlobal
    {
        [SettingsAttribute(Message ="Selected schema:", Position =1)]
        public string SelectedSchema { get; set; } = string.Empty;

        [SettingsAttribute(Message = "Auhtor name (used in code comments):", Position = 2)]
        public string AuthorName { get; set; } = "Dapper Layers Generator User";

        [SettingsAttribute(Message = "Target project namespace:", Position = 3)]
        public string TargetProjectNamespace { get; set; } = "MyTestProject";

        [SettingsAttribute(Message = "Target namespace for POCO (parent namespace is Project):", Position = 4, ChildOf = "TargetProjectNamespace")]
        public string TargetNamespaceForPOCO { get; set; } = "MyTestProject.POCO";

        [SettingsAttribute(Message = "Target namespace for Repo (parent namespace is Project):", Position = 5, ChildOf = "TargetProjectNamespace")]
        public string TargetNamespaceForRepo { get; set; } = "MyTestProject.Repo";

        [SettingsAttribute(Message = "Target namespace for DbContext (parent namespace is Project):", Position = 6,ChildOf = "TargetProjectNamespace")]
        public string TargetNamespaceForDbContext { get; set; } = "MyTestProject.DbContext";

        [SettingsAttribute(Message = "Target project path:", Position = 7)]
        public string TargetProjectPath { get; set; } = @"c:\temp\MyTestProject\";

        [SettingsAttribute(Message = "Target folder for POCO files:", Position = 8, ChildOf = "TargetProjectPath")]
        public string TargetFolderForPOCO { get; set; } = @"c:\temp\MyTestProject\POCO\";

        [SettingsAttribute(Message = "Target folder for repo files:", Position = 9, ChildOf = "TargetProjectPath")]
        public string TargetFolderForRepo { get; set; } = @"c:\temp\MyTestProject\Repo\";

        [SettingsAttribute(Message = "Target folder for DB contect file:", Position = 10, ChildOf = "TargetProjectPath")]
        public string TargetFolderForDBContext { get; set; } = @"c:\temp\MyTestProject\DbContext\";



        //Global table settings 
        public SettingsTable TableGlobalSettings { get; set; } = new SettingsTable();

        //Override table global seetings
        public Dictionary<string, SettingsTable> TableSettings { get; set; } = new Dictionary<string, SettingsTable>();

        public async Task SaveToFile(string configPath)
        {
            using FileStream createStream = File.Create(configPath);
            await JsonSerializer.SerializeAsync(createStream, this, new JsonSerializerOptions() { WriteIndented = true });
            await createStream.DisposeAsync();
        }

        public static async Task<SettingsGlobal?> LoadFromFile(string configPath)
        {
            using FileStream openStream = File.OpenRead(configPath);

            return openStream == null ? null : await JsonSerializer.DeserializeAsync<SettingsGlobal>(openStream);
        }

    }
}
