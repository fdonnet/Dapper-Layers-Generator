using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Generators;
using Dapper_Layers_Generator.Core.Generators.MySql;
using Dapper_Layers_Generator.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using MySqlX.XDevAPI.Relational;
using System;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Dapper_Layers_Generator.Core
{
    public interface IGeneratorService
    {
        SettingsGlobal GlobalGeneratorSettings { get; set; }
        Task GenerateAsync(IProgress<string> progress);
    }


    /// <summary>
    /// TODO NEEED to rewrite it is UGLY 
    /// </summary>
    public class GeneratorService : IGeneratorService
    {
        public SettingsGlobal GlobalGeneratorSettings { get; set; }


        private readonly IReaderDBDefinitionService _dataService;
        private readonly IGeneratorsProvider _generatorsProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly StringTransformationService _stringTransformation;
        private string _currentRepoSubDirectory = string.Empty;
        private string CurrentRepoSubDirectoryFullPath
        {
            get
            {
                return GlobalGeneratorSettings.TargetFolderForRepo + _currentRepoSubDirectory;
            }
        }

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
            Task repoTasks = BuildRepoTasks(scope, progress, selectedTableNames);
            Task pocoTasks = BuildPocoTasks(scope, progress, selectedTableNames);

            Task allTask = Task.WhenAll(contextTasks, repoTasks, pocoTasks);

            await allTask;
        }

        private Task BuildContextTasks(IServiceScope scope, IProgress<string> progress)
        {
            //Context
            //Base abstract context gen
            progress.Report("---- Context Generator BEGINS ----");
            var outpoutContextBase = GenerateOutput<IGeneratorContextBase>(scope);
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
                    outputContext = GenerateOutput<IMySqlGeneratorContext>(scope);
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

            List<string> outputBaseList;
            foreach (var tableName in selectedTableNames)
            {
                outputBaseList = new List<string>
                {
                    //Main Repo
                    GenerateOutput<IGeneratorRepoMain>(scope, tableName,false,true),

                    //Get all base
                    GenerateOutput<IGeneratorRepoGetAll>(scope, tableName, true),

                    //Get by PK base
                    GenerateOutput<IGeneratorRepoGetByPk>(scope, tableName, true),

                    //Get by PK list base
                    GenerateOutput<IGeneratorRepoGetByPkList>(scope, tableName, true),

                    //Get by UK base
                    GenerateOutput<IGeneratorRepoGetByUk>(scope, tableName, true),

                    //Add base
                    GenerateOutput<IGeneratorRepoAdd>(scope, tableName, true),

                    //Add Multi base
                    GenerateOutput<IGeneratorRepoAddMulti>(scope, tableName, true),

                    //Add bulk base
                    GenerateOutput<IGeneratorRepoAddBulk>(scope, tableName, true),

                    //Update base
                    GenerateOutput<IGeneratorRepoUpdate>(scope, tableName, true),

                    //Update multi base
                    GenerateOutput<IGeneratorRepoUpdateMulti>(scope, tableName, true),

                    //Update bulk base
                    GenerateOutput<IGeneratorRepoUpdateBulk>(scope, tableName, true),

                    //Delete base
                    GenerateOutput<IGeneratorRepoDelete>(scope, tableName, true),

                    //Delete by pklist base
                    GenerateOutput<IGeneratorRepoDeleteByPkList>(scope, tableName, true)
                };

                var outputbase = String.Join(string.Empty, outputBaseList) + $"{tab}}}{Environment.NewLine}}}"; ;

                var repoBaseTaskMain = WriteFileAsync($"{CurrentRepoSubDirectoryFullPath}" +
                                    $"{_stringTransformation.ApplyConfigTransformClass(tableName)}RepoBase.cs"
                                    , outputbase, "RepoGenerator", progress);

                tasksRepo.Add(repoBaseTaskMain);

                //DbProvider specific
                foreach (var dbType in GlobalGeneratorSettings.TargetDbProviderForGeneration.Split(','))
                {
                    List<string> outputDbSpecificList;

                    if (dbType == "MySql")
                    {
                        outputDbSpecificList = new List<string>
                        {
                            GenerateOutput<IMySqlGeneratorRepoMain>(scope, tableName),

                            //Get all
                            GenerateOutput<IMySqlGeneratorRepoGetAll>(scope, tableName, true),

                            //Get by pk
                            GenerateOutput<IMySqlGeneratorRepoGetByPk>(scope, tableName, true),

                            //Get by pk list
                            GenerateOutput<IMySqlGeneratorRepoGetByPkList>(scope, tableName, true),

                            //Get by uk 
                            GenerateOutput<IMySqlGeneratorRepoGetByUk>(scope, tableName, true),

                            //Add
                            GenerateOutput<IMySqlGeneratorRepoAdd>(scope, tableName, true),

                            //Add multi
                            GenerateOutput<IMySqlGeneratorRepoAddMulti>(scope, tableName, true),

                            //Add bulk
                            GenerateOutput<IMySqlGeneratorRepoAddBulk>(scope, tableName, true),

                            //Update
                            GenerateOutput<IMySqlGeneratorRepoUpdate>(scope, tableName, true),

                            //Update multi
                            GenerateOutput<IMySqlGeneratorRepoUpdateMulti>(scope, tableName, true),

                            //Update Bulk
                            GenerateOutput<IMySqlGeneratorRepoUpdateBulk>(scope, tableName, true),

                            //Delete
                            GenerateOutput<IMySqlGeneratorRepoDelete>(scope, tableName, true),

                            //Delete by pklist
                            GenerateOutput<IMySqlGeneratorRepoDeleteByPkList>(scope, tableName, true)
                        };

                        var outputMySql = String.Join(string.Empty, outputDbSpecificList) + $"{tab}}}{Environment.NewLine}}}";

                        var repoTaskMainMySql = WriteFileAsync($"{CurrentRepoSubDirectoryFullPath}" +
                                $"{_stringTransformation.ApplyConfigTransformClass(tableName)}Repo{dbType}.cs"
                                , outputMySql, "RepoGenerator", progress);

                        tasksRepo.Add(repoTaskMainMySql);

                    }
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

        private string GenerateOutput<T>(IServiceScope scope, string? tableName = null, bool addNewLine = false, bool createFolder = false)
        {
            IGenerator generator = tableName == null
                ? (IGenerator)_generatorsProvider.GetGenerator<T>(scope)!
                : (IGenerator)_generatorsProvider.GetGenerator<T>(tableName, scope)!;

            //Create sub folder for repo main generator
            if (createFolder && tableName != null)
                CreateRepoFolderStructure(_stringTransformation.ApplyConfigTransformClass(tableName)!);

            var output = generator.Generate();

            if (addNewLine && !String.IsNullOrEmpty(output))
                output += Environment.NewLine;

            return output;
        }

        private void CreateRepoFolderStructure(string repoBaseClassName)
        {
            _currentRepoSubDirectory = repoBaseClassName + Path.DirectorySeparatorChar;

            bool existsFolder = Directory.Exists(CurrentRepoSubDirectoryFullPath);
            if (!existsFolder)
            {
                Directory.CreateDirectory(CurrentRepoSubDirectoryFullPath);
            }
        }
    }
}
