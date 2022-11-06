using Dapper_Layers_Generator.Console.Helpers;
using Dapper_Layers_Generator.Core;
using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Converters.MySql;
using Dapper_Layers_Generator.Core.Generators;
using Dapper_Layers_Generator.Core.Generators.MySql;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.Reader;
using Dapper_Layers_Generator.Data.Reader.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

ServiceProvider? _builder = null;
string _dbProviderToReadDBDef = string.Empty;

IConfiguration? _config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddUserSecrets<Program>()
        .AddEnvironmentVariables()
        .Build();

IServiceCollection _services = new ServiceCollection()
    .AddSingleton(_config);


//Welcome and load/check config
WelcomeMsg();

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
    List<string> acceptedProvider = new() { "MySql" };

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
                        AnsiConsole.MarkupLine("[red]Cannot read your ConnectionStrings:Default connection string ...[/]");
                    }
                    else
                    {

                        schemas = _config["DB:Schemas"];

                        if (string.IsNullOrEmpty(schemas))
                        {
                            AnsiConsole.MarkupLine("[red]Cannot read your DB:Schemas config...[/]");
                        }
                        else
                        {
                            _dbProviderToReadDBDef = _config["DB:Provider"];
                            if (String.IsNullOrEmpty(_dbProviderToReadDBDef) || !acceptedProvider.Contains(_dbProviderToReadDBDef))
                            {
                                AnsiConsole.MarkupLine($"[red]Cannot read your DB:provider config..., " +
                                    $"or not accepted provider to read DB definitions (possible values: {String.Join(',',acceptedProvider)})[/]");
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
                                Thread.Sleep(1000);
                                isOk = true;
                            }
                            
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

void BuildServices()
{
    try
    {
        _builder = ServicesConfig(_dbProviderToReadDBDef, _services);
        AnsiConsole.MarkupLine("DbProvider loading: SUCCESS !");
        _builder!.GetRequiredService<SettingsGlobal>().TargetDbProviderForGeneration = _dbProviderToReadDBDef;
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
    }

    if (_builder == null)
    {
        AnsiConsole.WriteLine("Provider not supported or services problem, app will shut down (10sec) ...");
        Thread.Sleep(10000);
        Environment.Exit(0);
    }
}

ServiceProvider? ServicesConfig(string dbProvider, IServiceCollection services)
{
    //Always running services
    services.AddSingleton<IReaderDBDefinitionService, ReaderDBDefinitionService>();
    services.AddSingleton<SettingsGlobal>();
    services.AddSingleton<ConsoleService>();
    services.AddSingleton<IGeneratorsProvider, GeneratorsProvider>();

    
    //Available Generators or services (to be really scoped,
    //following generators will be call by Generator service and
    // Generator will request a IServiceScope.... to manage life cycle. 
    services.AddScoped<IGeneratorPOCO, GeneratorPOCO>();
    services.AddScoped<StringTransformationService>();
    services.AddScoped<IGeneratorService, GeneratorService>();
    services.AddScoped<IGeneratorContextBase, GeneratorContextForBase>();

    //MySql specific (db provider for source generation)
    //You will be able to generate the code for several db types... 
    services.AddScoped<IGeneratorRepoAdd, MySqlGeneratorRepoAdd>();
    services.AddScoped<IDataTypeConverter, MySqlDataTypeConverter>();
    services.AddScoped<IMySqlGeneratorContext, MySqlGeneratorContext>();

    //Service that depend on the dbprovider (in config to read db defintions)
    if (dbProvider == "MySql")
    {
        services.AddTransient<IReaderDapperContext, MysqlReaderDapperContext>();
    }

    //If accepted DB providers (for the moment only mysqlto read data def)
    return dbProvider == "MySql" ? services.BuildServiceProvider() : null;
}


