using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
internal partial class ConsoleService
{
    internal async Task ShowSelectTablesAsync()
    {
        AnsiConsole.Clear();
        var settings = _generatorService.GlobalGeneratorSettings;

        var tables = _dataService.SchemaDefinitions?.FirstOrDefault(s => s.Name == settings.SelectedSchema)?.Tables ?? new List<ITable>();

        if (settings.RunGeneratorForAllTables)
        {
            if (!AnsiConsole.Confirm("For the moment, all tables are selected for generation, continue like this (y) or select tables(n):"))
            {
                settings.RunGeneratorForSelectedTables = BuildSelectionTable(settings, tables);
                await SelectionSaved();
            }
            else
                await ReturnToMainMenuAsync();
        }
        else
        {
            var tableUI = new Table();
            tableUI.AddColumn("Table name");
            tableUI.AddColumn("Selected");

            foreach (var dbTable in tables)
            {
                tableUI.AddRow(dbTable.Name, settings.RunGeneratorForSelectedTables.Where(t => t == dbTable.Name).Any() ? "X" : "");
            }

            AnsiConsole.Write(tableUI);

            var ask = AnsiConsole.Ask<string>("You already selected the above tables, to clear and reselect press (c), or press (a) to select all, to keep press (k)");

            if(ask == "c")
            {
                settings.RunGeneratorForSelectedTables = BuildSelectionTable(settings, tables);
                await SelectionSaved();
            }
            else
            {
                if(ask == "a")
                {
                    settings.RunGeneratorForSelectedTables = new List<string>();
                    settings.RunGeneratorForAllTables = true;
                    await SelectionSaved();
                }
                else
                {
                    await ReturnToMainMenuAsync();
                }
                
            }

        }
    }

    private List<string> BuildSelectionTable(SettingsGlobal settings, IList<ITable> tables)
    {
        if (tables != null)
        {
            settings.RunGeneratorForAllTables = false;

            var tablesStrings = AnsiConsole.Prompt(
                                    new MultiSelectionPrompt<string>()
                                        .Title("Select the tables you want to generate?")
                                        .PageSize(10)
                                        .MoreChoicesText("[grey](Move up and down to reveal more tables)[/]")
                                        .InstructionsText(
                                            "[grey](Press [blue]<space>[/] to toggle a table, " +
                                            "[green]<enter>[/] to accept)[/]")
                                        .AddChoices(tables.Select(t => t.Name).OrderBy(t=>t)));
            return tablesStrings;

        }

        return new List<string>();
    }

    private async Task SelectionSaved()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine("Tables selection saved !");
        await ReturnToMainMenuAsync();
    }
}

