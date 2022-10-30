
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
Console.WriteLine(WelcomeMsg());

var dbProvider = Console.ReadLine();

//Services conf
dbProvider = dbProvider ?? "";
var builder = ServicesConfig(dbProvider, services);

if(builder != null)
{
    _consoleService = builder.GetRequiredService<ConsoleService>();

    //Summary
    Console.Clear();
    Console.WriteLine(_consoleService.BeginSummaryPrint(dbProvider));

    //Load DB definitions
    await LoadDBDefintionStep();

}
else
{
    Console.WriteLine(@$"Provider not supported or services problem, app will shut down ...");
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
        services.AddSingleton<ConsoleService>();
        return services.BuildServiceProvider();
    }

    return null;

}


//////////////////////////////////////////////////
//Console Steps
/////////////////////////////////////////////////

//Welcome
static string WelcomeMsg()
{
    return @$"
******************************************************
*** Welcome, you will generate layers from your DB ***
******************************************************

-- Choose your DB provider, (mysql) :";

}

//DB Loading
async Task LoadDBDefintionStep()
{
    Console.Clear();
    Console.WriteLine(await _consoleService.LoadDBDefinitionsAsync());
    var k = Console.ReadKey().Key;

    if (k == ConsoleKey.Y)
    {
        Console.Write(_consoleService.PrintDBDefintionAsync());
    }
}

