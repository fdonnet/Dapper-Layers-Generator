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
            var tab = _stringTransformation.IndentString;

            List<Task> tasksRepo = new();
            progress.Report(Environment.NewLine + "---- Repo Generator BEGINS ----");

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

                //Get all base
                var generatorGetAllBase = _generatorsProvider.GetGenerator<IGeneratorRepoGetAll>(tableName, scope);
                var outputGetAllBase = generatorGetAllBase.Generate();

                //Get by PK base
                outputGetAllBase = !string.IsNullOrEmpty(outputGetAllBase) ? outputGetAllBase + Environment.NewLine : string.Empty;
                var generatorGetByPkBase = _generatorsProvider.GetGenerator<IGeneratorRepoGetByPk>(tableName, scope);
                var outputGetByPkBase = generatorGetByPkBase.Generate();

                //Get by PK list base
                outputGetByPkBase = !string.IsNullOrEmpty(outputGetByPkBase) ? outputGetByPkBase + Environment.NewLine : string.Empty;
                var generatorGetByPkListBase = _generatorsProvider.GetGenerator<IGeneratorRepoGetByPkList>(tableName, scope);
                var outputGetByPkListBase = generatorGetByPkListBase.Generate();

                //Get by UK base
                outputGetByPkListBase = !string.IsNullOrEmpty(outputGetByPkListBase) ? outputGetByPkListBase + Environment.NewLine : string.Empty;
                var generatorGetByUkBase = _generatorsProvider.GetGenerator<IGeneratorRepoGetByUk>(tableName, scope);
                var outputGetByUkBase = generatorGetByUkBase.Generate();

                //Add base
                outputGetByUkBase = !string.IsNullOrEmpty(outputGetByUkBase) ? outputGetByUkBase + Environment.NewLine : string.Empty;
                var generatorAddBase = _generatorsProvider.GetGenerator<IGeneratorRepoAdd>(tableName, scope);
                var outputAddBase = generatorAddBase.Generate();

                //Update base
                outputAddBase = !string.IsNullOrEmpty(outputAddBase) ? outputAddBase + Environment.NewLine : string.Empty;
                var generatorUpdateBase = _generatorsProvider.GetGenerator<IGeneratorRepoUpdate>(tableName, scope);
                var outputUpdateBase = generatorUpdateBase.Generate();

                //Delete base
                outputUpdateBase = !string.IsNullOrEmpty(outputUpdateBase) ? outputUpdateBase + Environment.NewLine : string.Empty;
                var generatorDeleteBase = _generatorsProvider.GetGenerator<IGeneratorRepoDelete>(tableName, scope);
                var outputDeleteBase = generatorDeleteBase.Generate();

                outputRepoBaseMain = outputRepoBaseMain + outputGetAllBase + outputGetByPkBase + outputGetByPkListBase
                    + outputGetByUkBase + outputAddBase + outputUpdateBase + outputDeleteBase + $"{tab}}}{Environment.NewLine}}}";
                var repoBaseTaskMain = WriteFileAsync($"{subDirectoryFullPath}" +
                                    $"{generatorRepoBaseMain.ClassName}RepoBase.cs"
                                    , outputRepoBaseMain, "RepoGenerator", progress);

                tasksRepo.Add(repoBaseTaskMain);

                //DbProvider specific
                foreach (var dbType in GlobalGeneratorSettings.TargetDbProviderForGeneration.Split(','))
                {
                    string outputRepoMain = string.Empty;
                    var outputGetAllSpec = string.Empty;
                    var outputGetByPkSpec = string.Empty;
                    var outputGetByPkListSpec = string.Empty;
                    var outputGetByUkSpec = string.Empty;
                    var outputAddSpec = string.Empty;
                    var outputUpdateSpec = string.Empty;
                    var outputDeleteSpec = string.Empty;

                    string className = string.Empty;
                    
                    if (dbType == "MySql")
                    {
                        var generatorRepoMain = _generatorsProvider.GetGenerator<IMySqlGeneratorRepoMain>(tableName,scope);
                        outputRepoMain = generatorRepoMain.Generate();

                        className = generatorRepoMain.ClassName;

                        //Get all
                        var generatorGetAllSpec = _generatorsProvider.GetGenerator<IMySqlGeneratorRepoGetAll>(tableName, scope);
                        outputGetAllSpec = generatorGetAllSpec.Generate();

                        //Get by pk
                        outputGetAllSpec = !string.IsNullOrEmpty(outputGetAllSpec) ? outputGetAllSpec + Environment.NewLine : string.Empty;
                        var generatorGetByPkSpec = _generatorsProvider.GetGenerator<IMySqlGeneratorRepoGetByPk>(tableName, scope);
                        outputGetByPkSpec = generatorGetByPkSpec.Generate();

                        //Get by pk list
                        outputGetByPkSpec = !string.IsNullOrEmpty(outputGetByPkSpec) ? outputGetByPkSpec + Environment.NewLine : string.Empty;
                        var generatorGetByPkListSpec = _generatorsProvider.GetGenerator<IMySqlGeneratorRepoGetByPkList>(tableName, scope);
                        outputGetByPkListSpec = generatorGetByPkListSpec.Generate();

                        //Get by uk 
                        outputGetByPkListSpec = !string.IsNullOrEmpty(outputGetByPkListSpec) ? outputGetByPkListSpec + Environment.NewLine : string.Empty;
                        var generatorGetByUkSpec = _generatorsProvider.GetGenerator<IMySqlGeneratorRepoGetByUk>(tableName, scope);
                        outputGetByUkSpec = generatorGetByUkSpec.Generate();

                        //Add
                        outputGetByUkSpec = !string.IsNullOrEmpty(outputGetByUkSpec) ? outputGetByUkSpec + Environment.NewLine : string.Empty;
                        var generatorAddSpec = _generatorsProvider.GetGenerator<IMySqlGeneratorRepoAdd>(tableName, scope);
                        outputAddSpec = generatorAddSpec.Generate();

                        //Update
                        outputAddSpec = !string.IsNullOrEmpty(outputAddSpec) ? outputAddSpec + Environment.NewLine : string.Empty;
                        var generatorUpdateSpec = _generatorsProvider.GetGenerator<IMySqlGeneratorRepoUpdate>(tableName, scope);
                        outputUpdateSpec = generatorUpdateSpec.Generate();

                        //Delete
                        outputUpdateSpec = !string.IsNullOrEmpty(outputUpdateSpec) ? outputUpdateSpec + Environment.NewLine : string.Empty;
                        var generatorDeleteSpec = _generatorsProvider.GetGenerator<IMySqlGeneratorRepoDelete>(tableName, scope);
                        outputDeleteSpec = generatorDeleteSpec.Generate();
                    }

                    outputRepoMain = outputRepoMain + outputGetAllSpec + outputGetByPkSpec + outputGetByPkListSpec 
                        + outputGetByUkSpec + outputAddSpec + outputUpdateSpec + outputDeleteSpec + $"{tab}}}{Environment.NewLine}}}";

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
