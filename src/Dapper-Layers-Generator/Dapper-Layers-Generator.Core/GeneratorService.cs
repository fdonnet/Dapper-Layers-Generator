using Dapper_Layers_Generator.Core.Generators;
using Dapper_Layers_Generator.Core.Generators.MySql;
using Dapper_Layers_Generator.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

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
        private readonly IServiceProvider _serviceProvider;

        public GeneratorService(SettingsGlobal settingsGlobal
            , IGeneratorsProvider generatorsProvider
            , IReaderDBDefinitionService dataService
            , IServiceProvider serviceProvider)
        {
            GlobalGeneratorSettings = settingsGlobal;
            _dataService = dataService;
            _generatorsProvider = generatorsProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task GenerateAsync(IProgress<string> progress)
        {
            //To be really SCOPED
            using var scope = _serviceProvider.CreateScope();
            
            //Call to main generator to get selected tables from 
            var selectedTableNames = _generatorsProvider.GetGenerator<IGeneratorContextBase>(scope).GetSelectedTableNames();

            //Context
            //Base abstract context gen
            progress.Report("---- Context Generator BEGINS ----");
            var generatorContextBase = _generatorsProvider.GetGenerator<IGeneratorContextBase>(scope);
            var outpoutContextBase = generatorContextBase.Generate();
            var contextBaseTask = WriteFileAsync($"{GlobalGeneratorSettings.TargetFolderForDBContext}" +
                        $"{GlobalGeneratorSettings.DbContextClassName}Base.cs"
                        , outpoutContextBase, "ContextGeneratorBase", progress);

            //Real context db specific generation
            List<Task> tasksContextDB = new();
            foreach (var dbType in GlobalGeneratorSettings.TargetDbProviderForGeneration.Split(','))
            {
                string outputContext = string.Empty;
                if (dbType == "MySql")
                {
                    var generatorContext = _generatorsProvider.GetGenerator<IMySqlGeneratorContext>(scope);
                    outputContext = generatorContext.Generate();
                }
                
                var contextTask = WriteFileAsync($"{GlobalGeneratorSettings.TargetFolderForDBContext}" +
                                $"{GlobalGeneratorSettings.DbContextClassName}{dbType}.cs"
                                , outputContext, "ContextGenerator", progress);
                
                tasksContextDB.Add(contextTask);
            }
            Task dbContextSpecific = Task.WhenAll(tasksContextDB);
            Task contextTasks = Task.WhenAll(contextBaseTask, dbContextSpecific);

            //POCO
            List<Task> tasks = new();
            progress.Report(Environment.NewLine + "---- POCO Generator BEGINS ----");
            foreach(var tableName in selectedTableNames)
            {
                var generatorPoco = _generatorsProvider.GetGenerator<IGeneratorPOCO>(tableName,scope);
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
