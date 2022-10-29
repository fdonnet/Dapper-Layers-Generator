
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

using var builder = new ServiceCollection()
    .AddSingleton<IReaderDapperContext, MysqlReaderDapperContext>()
    .BuildServiceProvider();


var testMysql = builder.GetRequiredService<IReaderDapperContext>();


var tables = await testMysql.DatabaseDefinitionsRepo.GetAllTablesAsync();

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
