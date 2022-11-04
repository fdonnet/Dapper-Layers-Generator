using Dapper_Layers_Generator.Core.Generators;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using Dapper_Layers_Generator.Data.Reader;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Dapper_Layers_Generator.Core
{
    public interface IGeneratorService
    {
        SettingsGlobal GlobalGeneratorSettings { get; set; }
        Task GenerateAsync(IProgress<(double current, double total)>? progress = null);
    }

    public class GeneratorService : IGeneratorService
    {
        public SettingsGlobal GlobalGeneratorSettings { get; set; }
        private readonly IGeneratorsProvider _generatorsProvider;
        private readonly IReaderDBDefinitionService _dataService;

        public GeneratorService(SettingsGlobal settingsGlobal, IGeneratorsProvider generatorsProvider, IReaderDBDefinitionService dataService)
        {
            GlobalGeneratorSettings = settingsGlobal;
            _generatorsProvider = generatorsProvider;
            _dataService = dataService;
        }

        public async Task GenerateAsync(IProgress<(double current, double total)>? progress = null)
        {
            var selectedTableNames = new List<string>();
            if(GlobalGeneratorSettings.RunGeneratorForAllTables)
            {
                selectedTableNames = _dataService.SchemaDefinitions?.Where(s => s.Name == GlobalGeneratorSettings.SelectedSchema)
                                                                .SingleOrDefault()?.Tables?.ToList().Select(t => t.Name).ToList();

                if(selectedTableNames == null || selectedTableNames.Count == 0)
                {
                    throw new NullReferenceException("No db defintions found to generate anything...");
                }
            }
            else
            {
                selectedTableNames = GlobalGeneratorSettings.RunGeneratorForSelectedTables;
            }

            List<Task> tasks = new();
            foreach(var tableName in selectedTableNames)
            {
                //POCO
                var generator = _generatorsProvider.GetGenerator<IGeneratorPOCO>(tableName);
                var output = generator.Generate();
                tasks.Add(WriteFileAsync($"{GlobalGeneratorSettings.TargetFolderForPOCO}{generator.ClassName}.cs", output));
            }

            Task taskMain = Task.WhenAll(tasks);
            await taskMain;

        }

        private static async Task WriteFileAsync(string fileFullPath, string content)
        {

            byte[] buffer = Encoding.UTF8.GetBytes(content);
            int offset = 0;
            const int Buffer_Size = 5 * 1024 * 1024; // 5 MB;
            using var fileStream = new FileStream(fileFullPath,
            FileMode.Create, FileAccess.Write,
            FileShare.None, bufferSize: Buffer_Size, useAsync: true);
            await fileStream.WriteAsync(buffer.AsMemory(offset, buffer.Length));
        }
    }
}
