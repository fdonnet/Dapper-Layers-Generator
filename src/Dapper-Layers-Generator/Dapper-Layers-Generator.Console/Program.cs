
using Dapper.FluentMap;
using Dapper_Layers_Generator.Console;
using Dapper_Layers_Generator.Core;
using Dapper_Layers_Generator.Data.POCO.MySql;
using Dapper_Layers_Generator.Data.Reader;
using Dapper_Layers_Generator.Data.Reader.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Security;
using System;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

ConsoleService _consoleService;


//////////////////////////////////////////////////
//MAIN
/////////////////////////////////////////////////

//Configuration
IConfiguration config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .AddEnvironmentVariables()
        .Build();

var services = new ServiceCollection()
    .AddSingleton(config);

//Welcome (choose your DB provider)
var welcome = WelcomeMsg(config);
Console.WriteLine(welcome);

if (welcome.Contains("ERROR"))
    Environment.Exit(0);


//Read db provider choice
var dbProvider = Console.ReadLine();

//Services conf
dbProvider = dbProvider ?? "";
var builder = ServicesConfig(dbProvider, services);

//Cannot build the services (bye)
if (builder == null)
{
    Console.WriteLine(@$"Provider not supported or services problem, app will shut down ...");
    Environment.Exit(0);
}

//From now (services are runing, we can use the console service to print)
_consoleService = builder.GetRequiredService<ConsoleService>();

//Summary
Console.Clear();
Console.WriteLine(_consoleService.BeginSummaryPrint(dbProvider));

//Load DB definitions
await LoadDBDefinitionsStep();


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
        services.AddSingleton<ConsoleService>();
        return services.BuildServiceProvider();
    }

    return null;

}


//////////////////////////////////////////////////
//Console Steps
/////////////////////////////////////////////////

//Welcome
static string WelcomeMsg(IConfiguration config)
{
    var error = @$"
ERROR cannot read the config(connection string and or schemas choice)
ConnectionString: Default / DB:Schemas, app will exit";

    if (config != null)
    {
        try
        {
            var connectionStr = config.GetConnectionString("Default");
            var schemas = config["DB:Schemas"];

            if (!string.IsNullOrEmpty(connectionStr) && !string.IsNullOrEmpty(schemas))
                return @$"
******************************************************
*** Welcome, you will generate layers from your DB ***
******************************************************

DB connection: FOUND !
DB Schemas specified for the generator: {schemas}


-- Choose your DB provider, (mysql) :";
            else
                return error;
        }
        catch
        {
            return error;

        }
    }
    return error;
}

//DB Loading
async Task LoadDBDefinitionsStep()
{
    Console.Clear();
    Console.WriteLine(await _consoleService.LoadDBDefinitionsAsync());
    var k = Console.ReadKey().Key;

    if (k == ConsoleKey.Y)
    {
        Console.Write(_consoleService.PrintDBDefintionAsync());
    }
}

