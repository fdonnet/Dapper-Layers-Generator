using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using Spectre.Console;
using Table = Dapper_Layers_Generator.Data.POCO.Table;

internal partial class ConsoleService
{
    internal async Task ShowSelectTablesAsync()
    {
        AnsiConsole.Clear();
        var settings = _generatorService.GlobalGeneratorSettings;

        var tables = _dataService.SchemaDefinitions?.FirstOrDefault(s => s.Name == settings.SelectedSchema)?.Tables ?? new List<Table>();

        if (settings.RunGeneratorForAllTables)
        {
            if (!AnsiConsole.Confirm(@$"
For the moment, all tables are selected for generation (schema: [green]{settings.SelectedSchema}[/]).
Continue like this (y) or select tables(n):"))
            {
                settings.RunGeneratorForSelectedTables = BuildSelectionTable(settings, tables);
                await SelectionSavedAsync();
            }
            else
                await ReturnToMainMenuAsync();
        }
        else
        {
            var tableUI = new Spectre.Console.Table();
            tableUI.AddColumn("Table name");
            tableUI.AddColumn("Selected");

            foreach (var dbTable in tables)
            {
                tableUI.AddRow(dbTable.Name, settings.RunGeneratorForSelectedTables.Where(t => t == dbTable.Name).Any() ? "X" : "");
            }

            AnsiConsole.Write(tableUI);

            var ask = AnsiConsole.Ask<string>("You already selected the above tables, to revert and reselect press (r), or press (a) to select all, to keep press (k)");

            if(ask == "r")
            {
                settings.RunGeneratorForSelectedTables = BuildSelectionTable(settings, tables);
                await SelectionSavedAsync();
            }
            else
            {
                if(ask == "a")
                {
                    settings.RunGeneratorForSelectedTables = new List<string>();
                    settings.RunGeneratorForAllTables = true;
                    await SelectionSavedAsync();
                }
                else
                {
                    await ReturnToMainMenuAsync();
                }
                
            }

        }
    }

    private static List<string> BuildSelectionTable(SettingsGlobal settings, IList<Dapper_Layers_Generator.Data.POCO.Table> tables)
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

    private async Task SelectionSavedAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.WriteLine("Tables selection saved !");
        await ReturnToMainMenuAsync();
    }
}

