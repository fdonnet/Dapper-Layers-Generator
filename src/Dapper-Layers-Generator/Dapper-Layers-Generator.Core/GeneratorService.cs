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
        Task GenerateAsync(IProgress<string> progress);
    }

    public class GeneratorService : IGeneratorService
    {
        public SettingsGlobal GlobalGeneratorSettings { get; set; }
        private readonly IReaderDBDefinitionService _dataService;
        private readonly IGeneratorsProvider _generatorsProvider;

        public GeneratorService(SettingsGlobal settingsGlobal, IGeneratorsProvider generatorsProvider, IReaderDBDefinitionService dataService)
        {
            GlobalGeneratorSettings = settingsGlobal;
            _dataService = dataService;
            _generatorsProvider = generatorsProvider;
        }

        public async Task GenerateAsync(IProgress<string> progress)
        {
            //Call to main generator to get selected tables for 
            var selectedTableNames = _generatorsProvider.GetGenerator<IGeneratorContextBase>().GetSelectedTableNames();

            //Context
            //Base abstract context gen
            progress.Report("---- Context Generator BEGINS ----");
            var generatorContextBase = _generatorsProvider.GetGenerator<IGeneratorContextBase>();
            var outpoutContextBase = generatorContextBase.Generate();
            var contextBaseTask = WriteFileAsync($"{GlobalGeneratorSettings.TargetFolderForDBContext}" +
                        $"{GlobalGeneratorSettings.DbContextClassName}Base.cs"
                        , outpoutContextBase, "ContextGeneratorBase", progress);

            //Real context db specific generation
            var generatorContext = _generatorsProvider.GetGenerator<IGeneratorContext>();
            var outpoutContext = generatorContext.Generate();
            var contextTask = WriteFileAsync($"{GlobalGeneratorSettings.TargetFolderForDBContext}" +
                            $"{GlobalGeneratorSettings.DbContextClassName}{GlobalGeneratorSettings.DbProvider}.cs"
                            , outpoutContext, "ContextGenerator", progress);


            Task contextTasks = Task.WhenAll(contextBaseTask, contextTask);

            //POCO
            List<Task> tasks = new();
            progress.Report(Environment.NewLine + "---- POCO Generator BEGINS ----");
            foreach(var tableName in selectedTableNames)
            {
                var generatorPoco = _generatorsProvider.GetGenerator<IGeneratorPOCO>(tableName);
                var outputPoco = generatorPoco.Generate();
                tasks.Add(WriteFileAsync($"{GlobalGeneratorSettings.TargetFolderForPOCO}{generatorPoco.ClassName}.cs"
                        , outputPoco, "PocoGenerator", progress));
            }

            Task taskPoco= Task.WhenAll(tasks);


            Task allTask = Task.WhenAll(contextTasks, taskPoco);
            await allTask;
        }

        private static async Task WriteFileAsync(string fileFullPath, string content, string WriterType
            , IProgress<string> progress)
        {
            progress.Report($"{WriterType}: is writing {fileFullPath} ...");
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            int offset = 0;
            const int Buffer_Size = 5 * 1024 * 1024; // 5 MB;
            using var fileStream = new FileStream(fileFullPath,
            FileMode.Create, FileAccess.Write,
            FileShare.None, bufferSize: Buffer_Size, useAsync: true);
            await fileStream.WriteAsync(buffer.AsMemory(offset, buffer.Length));
            progress.Report($"{WriterType}: {fileFullPath} SUCCESS ...");
        }
    }
}
