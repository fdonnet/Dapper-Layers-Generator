using Dapper_Layers_Generator;
using Dapper_Layers_Generator.Core;
using Dapper_Layers_Generator.Data.Reader;
using Dapper_Layers_Generator.Data.Reader.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

string _dbProvider = string.Empty;
ServiceProvider? _builder = null;

IConfiguration? _config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .AddEnvironmentVariables()
        .Build();

IServiceCollection _services = new ServiceCollection()
    .AddSingleton(_config);


//Welcome and load/check config
WelcomeMsg();

//Choose dbprovider 
ProviderChoice();

//Build services to run the app
BuildServices();

//Run the real APP after base config (services etc)
var console = _builder!.GetRequiredService<ConsoleService>();
await console.InitAndLoadDbDefinitionsAsync();


//*****************************
//Function to init the apps
//*****************************
void WelcomeMsg()
{
    string schemas = string.Empty;
    bool isOk = false;

    ProgramHelper.MainTitle();

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
}

void BuildServices()
{
    try
    {
        _builder = ServicesConfig(_dbProvider, _services);
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
        services.AddSingleton<ConsoleService>();
        return services.BuildServiceProvider();
    }

    return null;

}
