using Dapper_Layers_Generator.Core;
using Dapper_Layers_Generator;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper_Layers_Generator.Core.Settings;

internal class MainMenu
{
    private ReaderDBDefinitionService _dataService = null!;
    private GeneratorService _generatorService = null!;
    private JsonSerializerOptions _jsonOption = new() { WriteIndented = true };
    private SettingsConfig _settingsConfig = null!;

    public MainMenu(ReaderDBDefinitionService dataService, GeneratorService generatorService, SettingsConfig settingsConfig)
    {
        _dataService = dataService;
        _generatorService = generatorService;
        _settingsConfig = settingsConfig;
    }

    internal async Task RunAsync()
    {
        await InitAndLoadDbDefinitions();
    }

    internal async Task ShowMenu()
    {
        AnsiConsole.Clear();
        ProgramHelper.MainTitle();

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
                await _settingsConfig.SeeGlobalSettings();
                break;
            case "Save settings to file":
                var pathSave = AnsiConsole.Ask<string>(@"Specify a complete filepath to save your config:");
                try
                {
                    await _generatorService.GlobalGeneratorSettings.SaveToFile(pathSave);
                    AnsiConsole.WriteLine("File saved !");
                    await ReturnToMain();
                }
                catch (Exception ex)
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
                await ShowMenu();
                break;
        }
    }

    private async Task InitAndLoadDbDefinitions()
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
                await PrintDbDefinitions();
            }
            else
                await ShowMenu();

        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            AnsiConsole.WriteLine(@$"Cannot read DB or print DB definition, app will shut down (10sec) ...");
            Thread.Sleep(10000);
            Environment.Exit(0);
        }

    }

    private async Task PrintDbDefinitions()
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

    private async Task ReturnToMain()
    {
        AnsiConsole.Write("Press any key to be redirected to the main menu");
        Console.ReadKey();
        await ShowMenu();
    }

}
