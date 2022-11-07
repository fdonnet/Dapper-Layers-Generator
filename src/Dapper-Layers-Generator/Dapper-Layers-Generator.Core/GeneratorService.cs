using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Generators;
using Dapper_Layers_Generator.Core.Generators.MySql;
using Dapper_Layers_Generator.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.Design.Serialization;
using System.Data;
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
        private readonly StringTransformationService _stringTransformation;

        public GeneratorService(SettingsGlobal settingsGlobal
            , IGeneratorsProvider generatorsProvider
            , IReaderDBDefinitionService dataService
            , IServiceProvider serviceProvider
            , StringTransformationService stringTransformation)
        {
            GlobalGeneratorSettings = settingsGlobal;
            _dataService = dataService;
            _generatorsProvider = generatorsProvider;
            _serviceProvider = serviceProvider;
            _stringTransformation = stringTransformation;
        }

        public async Task GenerateAsync(IProgress<string> progress)
        {
            //To be really SCOPED
            using var scope = _serviceProvider.CreateScope();
            
            //Call to main generator to get selected tables from 
            var selectedTableNames = _generatorsProvider.GetGenerator<IGeneratorContextBase>(scope).GetSelectedTableNames();

            //Tasks
            Task contextTasks = BuildContextTasks(scope, progress);
            Task repoTasks = BuildRepoTasks(scope, progress,selectedTableNames);
            Task pocoTasks = BuildPocoTasks(scope, progress,selectedTableNames);

            Task allTask = Task.WhenAll(contextTasks, repoTasks, pocoTasks);

            await allTask;
        }

        private Task BuildContextTasks(IServiceScope scope, IProgress<string> progress)
        {
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
            return Task.WhenAll(contextBaseTask, dbContextSpecific);
        }

        private Task BuildRepoTasks(IServiceScope scope, IProgress<string> progress, List<string> selectedTableNames)
        {

            List<Task> tasksRepo = new();
            progress.Report(Environment.NewLine + "---- Repo Generator BEGINS ----");
            var tab = _stringTransformation.IndentString;

            foreach (var tableName in selectedTableNames)
            {

                //Main Repo
                var generatorRepoBaseMain = _generatorsProvider.GetGenerator<IGeneratorRepoMain>(tableName, scope);
                //Create subfolder for each repo (can be messy if not if a lot of db providers)
                var subDirectoryFullPath=GlobalGeneratorSettings.TargetFolderForRepo
                    + generatorRepoBaseMain.ClassName
                    + Path.DirectorySeparatorChar;

                bool existsFolder = Directory.Exists(subDirectoryFullPath);
                if(!existsFolder)
                {
                    Directory.CreateDirectory(subDirectoryFullPath);
                }

                var outputRepoBaseMain = generatorRepoBaseMain.Generate();
                var generatorAddBase = _generatorsProvider.GetGenerator<IGeneratorRepoAdd>(tableName, scope);
                var outputAddBase = generatorAddBase.Generate();


                outputRepoBaseMain = outputRepoBaseMain + outputAddBase + $"{tab}}}{Environment.NewLine}}}";
                var repoBaseTaskMain = WriteFileAsync($"{subDirectoryFullPath}" +
                                    $"{generatorRepoBaseMain.ClassName}RepoBase.cs"
                                    , outputRepoBaseMain, "RepoGenerator", progress);
                tasksRepo.Add(repoBaseTaskMain);

                //DbProvider specific
                foreach (var dbType in GlobalGeneratorSettings.TargetDbProviderForGeneration.Split(','))
                {
                    string outputRepoMain = string.Empty;
                    var outputAddSpec = string.Empty;
                    string className = string.Empty;
                    if (dbType == "MySql")
                    {
                        var generatorRepoMain = _generatorsProvider.GetGenerator<IMySqlGeneratorRepoMain>(tableName,scope);
                        outputRepoMain = generatorRepoMain.Generate();
                        className = generatorRepoMain.ClassName;

                        var generatorAddSpec = _generatorsProvider.GetGenerator<IMySqlGeneratorRepoAdd>(tableName, scope);
                        outputAddSpec = generatorAddSpec.Generate();
                    }

                    outputRepoMain = outputRepoMain + outputAddSpec + $"{tab}}}{Environment.NewLine}}}";

                    var repoTaskMain = WriteFileAsync($"{subDirectoryFullPath}" +
                                $"{className}Repo{dbType}.cs"
                                , outputRepoMain, "RepoGenerator", progress);

                    tasksRepo.Add(repoTaskMain);
                }
            }

            return Task.WhenAll(tasksRepo);
        }

        private Task BuildPocoTasks(IServiceScope scope, IProgress<string> progress, List<string> selectedTableNames)
        {
            List<Task> tasks = new();
            progress.Report(Environment.NewLine + "---- POCO Generator BEGINS ----");
            foreach (var tableName in selectedTableNames)
            {
                var generatorPoco = _generatorsProvider.GetGenerator<IGeneratorPOCO>(tableName, scope);
                var outputPoco = generatorPoco.Generate();
                tasks.Add(WriteFileAsync($"{GlobalGeneratorSettings.TargetFolderForPOCO}{generatorPoco.ClassName}.cs"
                        , outputPoco, "PocoGenerator", progress));
            }

            return Task.WhenAll(tasks);
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
