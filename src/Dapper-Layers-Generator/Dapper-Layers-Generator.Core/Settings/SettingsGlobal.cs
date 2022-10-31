using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsGlobal :SettingsBase
    {
        public string OutputPath_ContextFile { get; set; } = "DapperLayersGenerator.Context.cs";
        public string Namespace_Context { get; set; } = "DapperLayersGenerator.Context";
        public bool RunGeneratorForAllSchemas { get; set; } = true;
        public IList<string> RunGeneratorForSelectedSchemas { get; set; } = new List<string>();

        //Global schema settings 
        public SettingsSchema SchemaGlobalSettings { get; set; } = new SettingsSchema();

        //Override schema global seetings
        public Dictionary<string, SettingsSchema> SchemaSettings { get; set; } = new Dictionary<string, SettingsSchema>();

        public async Task SaveToFile(string configPath)
        {
            using FileStream createStream = File.Create(configPath);
            await JsonSerializer.SerializeAsync(createStream, this, new JsonSerializerOptions() { WriteIndented = true });
            await createStream.DisposeAsync();
        }

        public static async Task<SettingsGlobal?> LoadFromFile(string configPath)
        {
            using FileStream openStream = File.OpenRead(configPath);
            
            if (openStream == null)
                return null;

            return  await JsonSerializer.DeserializeAsync<SettingsGlobal>(openStream);

        }

    }
}
