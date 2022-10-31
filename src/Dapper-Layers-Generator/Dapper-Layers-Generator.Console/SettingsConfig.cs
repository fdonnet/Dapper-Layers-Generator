using Dapper_Layers_Generator.Core;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class SettingsConfig
{
    private readonly GeneratorService _generatorService = null!;
    private readonly IConfiguration _config;

    public SettingsConfig(GeneratorService generatorService, IConfiguration config)
    {
        _generatorService = generatorService;
        _config = config;
    }

    internal async Task SeeGlobalSettings()
    {
        AnsiConsole.Clear();
        string extract = string.Empty;
        var schemas = _generatorService.GlobalGeneratorSettings.RunGeneratorForAllSchemas
                        ? _config["DB:Schemas"]
                        : _generatorService.GlobalGeneratorSettings.RunGeneratorForSelectedSchemas != null
                                    ? String.Join(",", _generatorService.GlobalGeneratorSettings.RunGeneratorForSelectedSchemas!)
                                    : "";

        AnsiConsole.WriteLine("Choose the value you want to change:");

        var table = new Table();
        // Add some columns
        table.AddColumn("Settings");
        table.AddColumn("Value");
        // Add some rows
        table.AddRow("1) Selected schemas to be generated:", schemas);
        table.AddRow("2) Author name:", _generatorService.GlobalGeneratorSettings.AuthorName);
        table.AddRow("3) Namespace used for the POCOs:", _generatorService.GlobalGeneratorSettings.Namespace_POCO);
        table.AddRow("4) Namespace used for the Repos:", _generatorService.GlobalGeneratorSettings.Namespace_Repo);
        table.AddRow("5) Namespace used for the Context:", _generatorService.GlobalGeneratorSettings.Namespace_Context);

        AnsiConsole.Write(table);

        var value = AnsiConsole.Ask<string>("Press the settings number you want to edit or (q) to return to main menu: ");

        if (value == "q")
            return;

        if (value != null && (new string[] { "1", "2", "3", "4", "5" }).Any(value.Contains))
        {
            switch (value)
            {
                case "1":
                    AnsiConsole.WriteLine("");
                    _generatorService.GlobalGeneratorSettings.RunGeneratorForSelectedSchemas =
                        AnsiConsole.Prompt(
                            new MultiSelectionPrompt<string>()
                                .Title("Choose at least one schema to be generated: ")
                                .AddChoices(_config["DB:Schemas"].Split(","))
                                .Required());

                    if (_generatorService.GlobalGeneratorSettings.RunGeneratorForSelectedSchemas.Count < _config["DB:Schemas"].Split(",").Length)
                        _generatorService.GlobalGeneratorSettings.RunGeneratorForAllSchemas = false;
                    else
                    {
                        _generatorService.GlobalGeneratorSettings.RunGeneratorForAllSchemas = true;
                        _generatorService.GlobalGeneratorSettings.RunGeneratorForSelectedSchemas = new List<string>();
                    }

                    break;
                case "2":
                    _generatorService.GlobalGeneratorSettings.AuthorName = AnsiConsole.Ask<string>("New author name");
                    break;

                case "3":
                    _generatorService.GlobalGeneratorSettings.Namespace_POCO = AnsiConsole.Ask<string>("New Namespace for POCO:");
                    break;
                case "4":
                    _generatorService.GlobalGeneratorSettings.Namespace_Repo = AnsiConsole.Ask<string>("New Namespace for repo:");
                    break;

                case "5":
                    _generatorService.GlobalGeneratorSettings.Namespace_Context = AnsiConsole.Ask<string>("New Namespace for Context:");
                    break;

                default:
                    await SeeGlobalSettings();
                    break;
            }

        }

        await SeeGlobalSettings();
    }
}
