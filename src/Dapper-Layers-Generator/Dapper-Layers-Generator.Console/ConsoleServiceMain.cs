using Dapper_Layers_Generator.Core;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper_Layers_Generator.Core.Settings;
using Microsoft.Extensions.Configuration;
using Dapper_Layers_Generator.Console.Helpers;

internal partial class ConsoleService
{
    private readonly ReaderDBDefinitionService _dataService = null!;
    private readonly GeneratorService _generatorService = null!;
    private readonly JsonSerializerOptions _jsonOption = new() { WriteIndented = true };
    private readonly IConfiguration _config;
    
    public ConsoleService(ReaderDBDefinitionService dataService, GeneratorService generatorService, IConfiguration config)
    {
        _dataService = dataService;
        _generatorService = generatorService;
        _config = config;
        _generatorService.GlobalGeneratorSettings.SelectedSchema = _config["DB:Schemas"].Split(",")[0];
    }

    internal async Task InitAndLoadDbDefinitionsAsync()
    {
        try
        {
            AnsiConsole.Clear();

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
                await PrintDbDefinitionsAsync();
            }
            else
                await ShowMainMenuAsync();

        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            AnsiConsole.WriteLine(@$"Cannot read DB or print DB definition, app will shut down (10sec) ...");
            Thread.Sleep(10000);
            Environment.Exit(0);
        }

    }

    internal async Task ReturnToMainMenuAsync()
    {
        AnsiConsole.Write("Main menu redirection, press any key");
        Console.ReadKey();
        await ShowMainMenuAsync();
    }

    internal async Task ShowMainMenuAsync()
    {
        AnsiConsole.Clear();
        ProgramHelper.MainTitle();

        var menu = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green]MENU[/]")
            .AddChoices(new[] {
                    "Re-print DB definition (JSON)",
                    "Select tables generation",
                    "Edit main settings",
                    "Edit global table settings",
                    "Advanced settings",
                    "Load config from file",
                    "Save config to file",
                    "Quit, don't forget to save your config !!!",
            }));

        switch (menu)
        {
            case "Re-print DB definition (JSON)":
                await PrintDbDefinitionsAsync();
                break;
            case "Select tables generation":
                await ShowSelectTablesAsync();
                break;
            case "Edit main settings":
                await ShowGlobalSettingsAsync();
                break;
            case "Edit global table settings":
                await ShowTableSettingsAsync();
                break;
            case "Advanced settings":
                await ShowTableSettingsAsync();
                break;
            case "Load config from file":
                var pathLoad = AnsiConsole.Ask<string>(@"Specify the complete filepath you want to load:");
                try
                {
                    _generatorService.GlobalGeneratorSettings = await SettingsGlobal.LoadFromFile(pathLoad) ?? new SettingsGlobal();
                    AnsiConsole.WriteLine("File loaded !");
                    await ReturnToMainMenuAsync();
                }
                catch (Exception ex)
                {
                    AnsiConsole.Write("Error during load !");
                    AnsiConsole.WriteException(ex);
                    await ReturnToMainMenuAsync();
                }
                break;
            case "Save config to file":
                var pathSave = AnsiConsole.Ask<string>(@"Specify a complete filepath to save your config:");
                try
                {
                    await _generatorService.GlobalGeneratorSettings.SaveToFile(pathSave);
                    AnsiConsole.WriteLine("File saved !");
                    await ReturnToMainMenuAsync();
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteLine("Error during save !");
                    AnsiConsole.WriteException(ex);
                    await ReturnToMainMenuAsync();
                }
                break;
            case "Quit, don't forget to save your config !!!":
                AnsiConsole.WriteLine("BYE !!!");
                Thread.Sleep(2000);
                Environment.Exit(0);
                break;
            default:
                await ShowMainMenuAsync();
                break;
        }
    }

    private async Task PrintDbDefinitionsAsync()
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

        await ReturnToMainMenuAsync();
    }

}
