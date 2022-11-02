using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

internal partial class ConsoleService
{
    internal async Task ShowAdvancedAsync()
    {
        AnsiConsole.Clear();
        var title = new Rule("Advanced/granular settings (go down at table lvl)");
        title.Centered();
        AnsiConsole.Write(title);
        AnsiConsole.WriteLine(string.Empty);

        var settings = _generatorService.GlobalGeneratorSettings;

        var selectedTables = _dataService.SchemaDefinitions?.FirstOrDefault(s => s.Name ==
            settings.SelectedSchema)?.Tables ?? new List<ITable>();

        //Exclude if not selected for generation
        if (!settings.RunGeneratorForAllTables)
        {
            selectedTables = selectedTables.Where(t => settings.RunGeneratorForSelectedTables.Contains(t.Name)).ToList();
        }

        var selectedTableName = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                        .Title("Select the table you want to configure: ")
                        .PageSize(20)
                        .MoreChoicesText("[grey](Move up and down to reveal more tables)[/]")
                        .AddChoices(selectedTables.Select(t => t.Name)));

        //Get if global or advanced settings
        if (!settings.TableSettings.TryGetValue(selectedTableName, out SettingsTable? curAdvSettings))
        {
            if(AnsiConsole.Confirm("For the moment, this table uses the global/normal settings, do you want to go with advanced/specific settings"))
            {
                settings.TableSettings.Add(selectedTableName, CopyGlobalTableSettings());
                curAdvSettings = settings.TableSettings[selectedTableName];
            }
            else
            {
                await ReturnToMainMenuAsync();
            }
        }

        await ShowTableSettingsAsync(true,curAdvSettings, selectedTableName);

    }

    internal async Task ShowAdvancedColumnsAsync(SettingsTable advancedSettings, string tableName)
    {
        AnsiConsole.Clear();
        var title = new Rule("Advanced/granular settings (go down at column lvl)");
        title.Centered();
        AnsiConsole.Write(title);
        AnsiConsole.WriteLine(string.Empty);

        var selectedColums = _dataService.SchemaDefinitions?.FirstOrDefault(s => s.Name ==
            _generatorService.GlobalGeneratorSettings.SelectedSchema)?.Tables?.Where(t => t.Name == tableName).SingleOrDefault()?.Columns;

        if (selectedColums == null)
        {
            AnsiConsole.WriteLine($"Cannot locate the columns of table {tableName}");
            await ReturnToMainMenuAsync();
        }

        var selectedColumnName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Select the column you want to configure: ")
                .PageSize(20)
                .MoreChoicesText("[grey](Move up and down to reveal more columns)[/]")
                .AddChoices(selectedColums!.Select(t => t.Name)));

        //Get if global or advanced settings
        if (!advancedSettings.ColumnSettings.TryGetValue(selectedColumnName, out SettingsColumn? curAdvSettings))
        {
            if (AnsiConsole.Confirm("For the moment, this column uses the table normal settings, do you want to go with advanced/specific settings"))
            {
                advancedSettings.ColumnSettings.Add(selectedColumnName, CopyGlobalColumnSettings(advancedSettings));
                curAdvSettings = advancedSettings.ColumnSettings[selectedColumnName];
            }
            else
            {
                await ShowTableSettingsAsync(true, advancedSettings, tableName);
            }
        }

        //await (true, curAdvSettings, selectedTableName);

    }


    private SettingsTable CopyGlobalTableSettings()
    {
        var newSettingCopy = JsonSerializer.Serialize(_generatorService.GlobalGeneratorSettings.TableGlobalSettings);
        return JsonSerializer.Deserialize<SettingsTable>(newSettingCopy) ?? new SettingsTable();
    }

    private static SettingsColumn CopyGlobalColumnSettings(SettingsTable curTableSettings)
    {
        var newSettingCopy = JsonSerializer.Serialize(curTableSettings.ColumnGlobalSettings);
        return JsonSerializer.Deserialize<SettingsColumn>(newSettingCopy) ?? new SettingsColumn();
    }

}

