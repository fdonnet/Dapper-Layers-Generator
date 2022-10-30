
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


IConfiguration config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .AddEnvironmentVariables()
        .Build();

var conf = new ServiceCollection()
    .AddSingleton(config);

Console.WriteLine(

@$"Welcome, you will generate layers from your DB
-- Choose your DB provider, (mysql) :");

var dbProvider = Console.ReadLine();

//Inject the correct context based on user response
if(dbProvider == "mysql")
{
    conf.AddTransient<IReaderDapperContext, MysqlReaderDapperContext>();
}

//Check if provider is a valid value
if (dbProvider == "mysql")
{
    conf.AddSingleton<ReaderDBDefinitionService>();
    conf.AddSingleton<ConsoleService>();
    var builder = conf.BuildServiceProvider();
    _consoleService = builder.GetRequiredService<ConsoleService>();
    
    //Summary
    Console.Clear();
    Console.WriteLine(_consoleService.BeginSummaryPrint(dbProvider));

    //Load DB definitions
    await LoadDBDefintionStep();

}
else
{
    Console.WriteLine(@$"Provider not supported, app will shut down ...");
}


//Console Steps
async Task LoadDBDefintionStep()
{
    Console.Clear();
    Console.WriteLine(await _consoleService.LoadDBDefinitions());
    var k = Console.ReadKey();



}



