
using Dapper.FluentMap;
using Dapper_Layers_Generator.Data.POCO.MySql;
using Dapper_Layers_Generator.Data.Reader;
using Dapper_Layers_Generator.Data.Reader.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using static System.Net.Mime.MediaTypeNames;


IConfiguration config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .AddEnvironmentVariables()
        .Build();

var conf = new ServiceCollection()
    .AddSingleton(config);

Console.WriteLine("Welcome, you will generate layers from your DB" + Environment.NewLine + "Choose your DB provider, (mysql) :");

var dbProvider = Console.ReadLine();

if(dbProvider == "mysql")
{
    conf.AddTransient<IReaderDapperContext, MysqlReaderDapperContext>();
}

var builder = conf.BuildServiceProvider();


var testMysql = builder.GetRequiredService<IReaderDapperContext>();
var tables = await testMysql.DatabaseDefinitionsRepo.GetAllTablesAsync();

Console.WriteLine("List of tables:");
foreach(var table in tables)
{
    Console.WriteLine(table.Schema + " " + table.Name);
}

// See https://aka.ms/new-console-template for more information

