
using Dapper.FluentMap;
using Dapper_Layers_Generator.Core;
using Dapper_Layers_Generator.Data.POCO.MySql;
using Dapper_Layers_Generator.Data.Reader;
using Dapper_Layers_Generator.Data.Reader.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Security;
using Spectre.Console;
using System;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

string _dbProvider = string.Empty;
ServiceProvider? _builder = null;
IConfiguration? _config = null;
ReaderDBDefinitionService? _dataService = null;

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


//////////////////////////////////////////////////
//Console Steps
/////////////////////////////////////////////////

void WelcomeMsg()
{
    //var errMsg = @$"
    //ERROR cannot read the config(connection string and or schemas choice)
    //ConnectionString: Default / DB:Schemas, app will exit";

    string schemas = string.Empty;
    bool isOk = false;

    AnsiConsole.Write(
        new FigletText("Dapper Layers Generator")
        .Centered());

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

async Task InitAndLoadDbDefinitions()
{
    try
    {
        AnsiConsole.Clear();

        _dataService = _builder!.GetRequiredService<ReaderDBDefinitionService>();

        await AnsiConsole.Status()
            .StartAsync(
                "Loading DB definitions...",
                    _ =>  _dataService.ReadAllDBDefinitionsStepAsync());

        AnsiConsole.WriteLine("DB definitions loaded.");
        if (AnsiConsole.Confirm("Do you want to print all definitions ?"))
        {
            var option = new JsonSerializerOptions() { WriteIndented = true };
            string extract = string.Empty;
            AnsiConsole.Status()
            .Start("Printing DB definitions...", ctx =>
            {
                extract = JsonSerializer.Serialize(_dataService.SchemaDefinitions, option);
                AnsiConsole.WriteLine(extract);

                AnsiConsole.MarkupLine("Print finished !");
            });
        }

    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
        AnsiConsole.WriteLine(@$"Cannot read DB or print DB definition, app will shut down (10sec) ...");
        Thread.Sleep(10000);
        Environment.Exit(0);
    }

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
        return services.BuildServiceProvider();
    }

    return null;

}

