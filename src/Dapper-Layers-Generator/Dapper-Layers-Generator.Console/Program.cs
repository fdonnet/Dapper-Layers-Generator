using Dapper_Layers_Generator.Core;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO.MySql;
using Dapper_Layers_Generator.Data.Reader;
using Dapper_Layers_Generator.Data.Reader.MySql;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Security;
using Spectre.Console;
using System;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

string _dbProvider = string.Empty;
ServiceProvider? _builder = null;
IConfiguration? _config = null;
ReaderDBDefinitionService? _dataService = null;
GeneratorService? _generatorService = null;
var _jsonOption = new JsonSerializerOptions() { WriteIndented = true };

//////////////////////////////////////////////////
//MAIN
/////////////////////////////////////////////////

//Configuration
_config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .AddEnvironmentVariables()
        .Build();

var services = new ServiceCollection()
    .AddSingleton(_config);

//Welcome
WelcomeMsg();

//Choose dbprovider and build service
ProviderChoice();

////Load DB definitions
await InitAndLoadDbDefinitions();

//====> Go in the main menu section

//END OF PROG---------------------------------------------------


//////////////////////////////////////////////////
//Console functions
/////////////////////////////////////////////////

//INIT PART/////////////////////////////////////////////
void WelcomeMsg()
{
    //var errMsg = @$"
    //ERROR cannot read the config(connection string and or schemas choice)
    //ConnectionString: Default / DB:Schemas, app will exit";

    string schemas = string.Empty;
    bool isOk = false;

    MainTitle();

    AnsiConsole.Status()
        .Start("Loading your connection string in config", ctx =>
        {
            try
            {
                if (_config != null)
                {
                    if (string.IsNullOrEmpty(_config.GetConnectionString("Default")))
                    {
                        AnsiConsole.MarkupLine("Cannot read your ConnectionStrings:Default connection string ...");
                    }
                    else
                    {

                        schemas = _config["DB:Schemas"];

                        if (string.IsNullOrEmpty(schemas))
                        {
                            AnsiConsole.MarkupLine("Cannot read your DB:Schemas config string ...");
                        }
                        else
                        {
                            var info =
$@"
Connection string loading: SUCCESS !
Schemas specified:  {schemas}
";
                            var panel = new Panel(info);
                            AnsiConsole.Write(panel);

                            isOk = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isOk = false;
                AnsiConsole.WriteException(ex);
            }
        });

    if (!isOk)
    {
        AnsiConsole.MarkupLine($"App will exit (10sec) ...");
        Thread.Sleep(10000);
        Environment.Exit(0);
    }
}

void ProviderChoice()
{
    _dbProvider = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Choose your dbprovider:")
        .PageSize(10)
        .AddChoices(new[] {
            "mysql",
        }));

    try
    {
        _builder = ServicesConfig(_dbProvider, services);
        AnsiConsole.MarkupLine("DbProvider loading: SUCCESS !");
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
    }

    if (_builder == null)
    {
        AnsiConsole.WriteLine(@$"Provider not supported or services problem, app will shut down (10sec) ...");
        Thread.Sleep(10000);
        Environment.Exit(0);
    }

}

void MainTitle()
{
    AnsiConsole.Write(
    new FigletText("Dapper Layers Generator")
    .Centered());
}

async Task InitAndLoadDbDefinitions()
{
    try
    {
        AnsiConsole.Clear();

        _dataService = _builder!.GetRequiredService<ReaderDBDefinitionService>();

        await AnsiConsole.Status()
             .StartAsync(
                 "Loading DB definitions...",
                      async ctx =>
                     {
                         await _dataService.ReadAllDBDefinitionsStepAsync();
                     });

        AnsiConsole.WriteLine("DB definitions loaded.");
        if (AnsiConsole.Confirm("Do you want to print all definitions ?", false))
        {
            await PrintDbDefinitions();
        }
        else
            await MainMenu();

    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
        AnsiConsole.WriteLine(@$"Cannot read DB or print DB definition, app will shut down (10sec) ...");
        Thread.Sleep(10000);
        Environment.Exit(0);
    }

}

async Task PrintDbDefinitions()
{
    AnsiConsole.Clear();

    string extract = string.Empty;
    AnsiConsole.Status()
    .Start("Printing DB definitions...", ctx =>
    {
        extract = JsonSerializer.Serialize(_dataService!.SchemaDefinitions, _jsonOption);
        AnsiConsole.WriteLine(extract);
        AnsiConsole.MarkupLine("Print finished !");
    });

    await ReturnToMain();
}


//MAIN MENU PART/////////////////////////////////////////////
async Task MainMenu()
{
    _generatorService = _builder!.GetRequiredService<GeneratorService>();

    AnsiConsole.Clear();
    MainTitle();

    var menu = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("MENU")
        .AddChoices(new[] {
            "Re-print DB definition (JSON)",
            "Load settings from file",
            "See global settings",
            "Save settings to file",
            "Quit, don't forget to save your config !!!",
        }));

    switch (menu)
    {
        case "Re-print DB definition (JSON)":
            await PrintDbDefinitions();
            break;
        case "Load settings from file":
            var pathLoad = AnsiConsole.Ask<string>(@"Specify the complete filepath you want to load:");
            try
            {
                _generatorService.GlobalGeneratorSettings = await SettingsGlobal.LoadFromFile(pathLoad) ?? new SettingsGlobal();
                AnsiConsole.WriteLine("File loaded !");
                await ReturnToMain();
            }
            catch (Exception ex)
            {
                AnsiConsole.Write("Error during save !");
                AnsiConsole.WriteException(ex);
                await ReturnToMain();
            }
            break;
        case "See global settings":
            await SeeGlobalSettings();
            break;
        case "Save settings to file":
            var pathSave = AnsiConsole.Ask<string>(@"Specify a complete filepath to save your config:");
            try
            {
                await _generatorService.GlobalGeneratorSettings.SaveToFile(pathSave);
                AnsiConsole.WriteLine("File saved !");
                await ReturnToMain();
            }
            catch(Exception ex)
            {
                AnsiConsole.WriteLine("Error during save !");
                AnsiConsole.WriteException(ex);
                await ReturnToMain();
            }
            break;
        case "Quit, don't forget to save your config !!!":
            AnsiConsole.WriteLine("BYE !!!");
            Thread.Sleep(2000);
            Environment.Exit(0);
            break;
        default:
            await MainMenu();
            break;
    }

}

async Task SeeGlobalSettings()
{
    AnsiConsole.Clear();
    string extract = string.Empty;
    var schemas = _generatorService.GlobalGeneratorSettings.RunGeneratorForAllSchemas
                    ? _config["DB:Schemas"]
                    : _generatorService.GlobalGeneratorSettings.RunGeneratorForSelectedSchemas != null
                                ? String.Join(",", _generatorService.GlobalGeneratorSettings.RunGeneratorForSelectedSchemas!)
                                : "";

    AnsiConsole.WriteLine("Choose the value you want to change:");

    var table = new Table();
    // Add some columns
    table.AddColumn("Settings");
    table.AddColumn("Value");
    // Add some rows
    table.AddRow("1) Selected schemas to be generated:", schemas);
    table.AddRow("2) Author name:", _generatorService.GlobalGeneratorSettings.AuthorName);
    table.AddRow("3) Namespace used for the POCOs:", _generatorService.GlobalGeneratorSettings.Namespace_POCO);
    table.AddRow("4) Namespace used for the Repos:", _generatorService.GlobalGeneratorSettings.Namespace_Repo);
    table.AddRow("5) Namespace used for the Context:", _generatorService.GlobalGeneratorSettings.Namespace_Context);

    AnsiConsole.Write(table);

    var value = AnsiConsole.Ask<string>("Press the settings number you want to edit or (q) to return to main menu: ");

    if (value == "q")
        await MainMenu();

    if (value != null && (new string[] { "1", "2", "3", "4", "5" }).Any(value.Contains))
    {
        switch (value)
        {
            case "1":
                AnsiConsole.WriteLine("");
                _generatorService.GlobalGeneratorSettings.RunGeneratorForSelectedSchemas =
                    AnsiConsole.Prompt(
                        new MultiSelectionPrompt<string>()
                            .Title("Choose at least one schema to be generated: ")
                            .AddChoices(_config["DB:Schemas"].Split(","))
                            .Required());

                if (_generatorService.GlobalGeneratorSettings.RunGeneratorForSelectedSchemas.Count < _config["DB:Schemas"].Split(",").Count())
                    _generatorService.GlobalGeneratorSettings.RunGeneratorForAllSchemas = false;
                else
                {
                    _generatorService.GlobalGeneratorSettings.RunGeneratorForAllSchemas = true;
                    _generatorService.GlobalGeneratorSettings.RunGeneratorForSelectedSchemas = new List<string>();
                }

                break;
            case "2":
                _generatorService.GlobalGeneratorSettings.AuthorName = AnsiConsole.Ask<string>("New author name");
                break;

            case "3":
                _generatorService.GlobalGeneratorSettings.Namespace_POCO = AnsiConsole.Ask<string>("New Namespace for POCO:");
                break;
            case "4":
                _generatorService.GlobalGeneratorSettings.Namespace_Repo = AnsiConsole.Ask<string>("New Namespace for repo:");
                break;

            case "5":
                _generatorService.GlobalGeneratorSettings.Namespace_Context = AnsiConsole.Ask<string>("New Namespace for Context:");
                break;

            default:
                await SeeGlobalSettings();
                break;
        }

    }

    await SeeGlobalSettings();
}

async Task ReturnToMain()
{
    AnsiConsole.Write("Press any key to be redirected to the main menu");
    Console.ReadKey();
    await MainMenu();
}

//////////////////////////////////////////////////
//Services config
/////////////////////////////////////////////////
ServiceProvider? ServicesConfig(string dbProvider, IServiceCollection services)
{
    //MySql
    if (dbProvider == "mysql")
    {
        services.AddTransient<IReaderDapperContext, MysqlReaderDapperContext>();
    }

    //If accepted dbProvider
    if (dbProvider == "mysql")
    {
        services.AddSingleton<ReaderDBDefinitionService>();
        services.AddSingleton<GeneratorService>();
        return services.BuildServiceProvider();
    }

    return null;

}

