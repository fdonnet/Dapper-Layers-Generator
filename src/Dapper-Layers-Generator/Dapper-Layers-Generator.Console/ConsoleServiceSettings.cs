using Dapper_Layers_Generator.Console.Helpers;
using Dapper_Layers_Generator.Core;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using MySqlX.XDevAPI.Relational;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

internal partial class ConsoleService
{
    internal async Task ShowGlobalSettingsAsync()
    {
        InitSettingsUI("Main configuration");

        var globalSettings = _generatorService.GlobalGeneratorSettings;

        //Set first schema from config
        globalSettings.SelectedSchema = string.IsNullOrEmpty(_generatorService.GlobalGeneratorSettings.SelectedSchema)
             ? globalSettings.SelectedSchema = _config["DB:Schemas"].Split(",")[0]
             : globalSettings.SelectedSchema;

        //Init settings div
        var dic = UISettingsHelper.SettingsDic(globalSettings);

        InitTable(dic, "Config values");

        var intValue = await ManageUserInputForGlobalSettingsAsync();

        //Change settings values
        if (dic.ContainsKey(intValue))
        {
            var newValue = string.Empty;

            switch (dic[intValue].PropertyName)
            {
                case "SelectedSchema":
                    AnsiConsole.WriteLine("");
                    newValue = AnsiConsole.Prompt(
                             new SelectionPrompt<string>()
                                 .Title("Choose at least one schema to be generated: ")
                                 .AddChoices(_config["DB:Schemas"].Split(",")));
                    globalSettings = (SettingsGlobal)UISettingsHelper.SetSettingsStringValue(globalSettings, dic[intValue].PropertyName, newValue);
                    break;
                case "TargetProjectNamespace":
                case "TargetProjectPath":
                    newValue = AnsiConsole.Ask<string>(dic[intValue].Label.Split(") ")[1]);
                    if (AnsiConsole.Confirm("Do you want to try to update child namespaces"))
                    {
                        var tmpDic = dic.Where(d => d.Value.ChildOf == dic[intValue].PropertyName);
                        foreach (var item in tmpDic)
                        {
                            var oldChildValue = item.Value.Settings;
                            if (oldChildValue.ToString()!.Contains(dic[intValue].Settings.ToString()!))
                            {
                                var newChildValue = oldChildValue.ToString()!.Replace(dic[intValue].Settings.ToString()!, newValue);
                                globalSettings = (SettingsGlobal)UISettingsHelper.SetSettingsStringValue(globalSettings, item.Value.PropertyName, newChildValue);
                            }
                        }
                    }
                    globalSettings = (SettingsGlobal)UISettingsHelper.SetSettingsStringValue(globalSettings, dic[intValue].PropertyName, newValue);
                    break;
                case "IndentStringInGeneratedCode":
                    newValue = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("What's your favorite indent way")
                                .AddChoices(new[] {
                                    "space", "double space","quadruple space", "tab", 
                                }));

                    globalSettings = (SettingsGlobal)UISettingsHelper.SetSettingsStringValue(globalSettings, dic[intValue].PropertyName, newValue);
                    break;
                default:
                    ChangeValue<SettingsGlobal>(dic[intValue], globalSettings);
                    break;

            }
        }

        await ShowGlobalSettingsAsync();
    }

    internal async Task ShowTableSettingsAsync(bool advancedMode = false, SettingsTable? advancedSettings = null, string tableName = "")
    {
        InitSettingsUI("Tables and columns " + (advancedMode ? "advanced" : "global") + " settings");

        if (advancedMode && (advancedSettings == null || tableName == string.Empty))
        {
            AnsiConsole.WriteLine("Cannot use the program this way...");
            await ReturnToMainMenuAsync();
        }

        //Global tab settings
        var tableSettings = advancedMode
            ? advancedSettings!
            : _generatorService.GlobalGeneratorSettings.TableGlobalSettings;

        if (advancedMode)
        {
            AnsiConsole.WriteLine(string.Empty);
            AnsiConsole.MarkupInterpolated($"TABLE NAME: [red]{tableName}[/]");
            AnsiConsole.WriteLine(string.Empty);
        }

        var dicTable = UISettingsHelper.SettingsDic(tableSettings);
        InitTable(dicTable, "Table settings");

        AnsiConsole.WriteLine(string.Empty);

        //Global col settings
        var colSettings = advancedMode
            ? advancedSettings!.ColumnGlobalSettings
            : _generatorService.GlobalGeneratorSettings.TableGlobalSettings.ColumnGlobalSettings;

        var dicColumns = UISettingsHelper.SettingsDic(colSettings);
        InitTable(dicColumns, "Columns settings");

        //If sub specific config
        AnsiConsole.WriteLine(string.Empty);
        if (advancedMode)
        {
            AnsiConsole.WriteLine("List of columns with a specific config");
            AnsiConsole.WriteLine(String.Join(",", advancedSettings!.ColumnSettings.Select(c => c.Key)));
        }
        else
        {
            AnsiConsole.WriteLine("List of tables with a specific config");
            AnsiConsole.WriteLine(String.Join(",", _generatorService.GlobalGeneratorSettings!.TableSettings.Select(c => c.Key)));
        }
        AnsiConsole.WriteLine(string.Empty);

        //Get the value
        var intValue = advancedMode
            ? await ManageUserInputForAdvancedSettingsAsync(tableSettings, tableName)
            : await ManageUserInputForTableSettingsAsync();




        //Change settings values
        if (dicTable.ContainsKey(intValue))
        {
            if (advancedMode && dicTable[intValue].IsColumnListChoice)
            {
                var possibleColumns = _dataService.SchemaDefinitions?.FirstOrDefault(s => s.Name == _generatorService.GlobalGeneratorSettings.SelectedSchema)
                                        ?.Tables?.Where(t => t.Name == tableName).SingleOrDefault()?.Columns?.OrderBy(c => c.Position).Select(c => c.Name);

                if (possibleColumns != null)
                {
                    AnsiConsole.WriteLine("");
                    var colString = AnsiConsole.Prompt(
                        new MultiSelectionPrompt<string>()
                            .Title("Select the columns you want for this specific config")
                            .PageSize(15)
                            .Required(false)
                            .MoreChoicesText("[grey](Move up and down to reveal more tables)[/]")
                            .InstructionsText(
                                "[grey](Press [blue]<space>[/] to toggle a table, " +
                                "[green]<enter>[/] to accept)[/]")
                            .AddChoices(possibleColumns));

                    UISettingsHelper.SetSettingsStringValue(tableSettings, dicTable[intValue].PropertyName, String.Join(",", colString));

                }
                else
                {
                    ChangeValue<SettingsTable>(dicTable[intValue], tableSettings);
                }

            }
            else
                ChangeValue<SettingsTable>(dicTable[intValue], tableSettings);
        }
        else
        {
            if (dicColumns.ContainsKey(intValue))
            {
                ChangeValue<SettingsColumn>(dicColumns[intValue], colSettings);
            }
        }

        await ShowTableSettingsAsync(advancedMode, tableSettings, tableName);
    }

    internal async Task ShowColumnSettingsAsync(SettingsColumn settingsColumn, string tableName, string columnName)
    {
        InitSettingsUI($"Column {tableName}.{columnName} settings");

        AnsiConsole.WriteLine(string.Empty);
        AnsiConsole.MarkupInterpolated($"TABLE NAME: [red]{tableName}[/] -- COLUMN NAME: [red]{columnName}[/]");
        AnsiConsole.WriteLine(string.Empty);

        var dicCol = UISettingsHelper.SettingsDic(settingsColumn);

        InitTable(dicCol, "Config values", true);

        var intValue = await ManageUserInputForAdvancedColSettingsAsync(settingsColumn, tableName, columnName);

        //Change settings values
        if (dicCol.ContainsKey(intValue))
        {
            ChangeValue<SettingsColumn>(dicCol[intValue], settingsColumn);
        }

        await ShowColumnSettingsAsync(settingsColumn, tableName, columnName);

    }

    private static void InitSettingsUI(string settingsType)
    {
        AnsiConsole.Clear();
        var title = new Rule("SETTINGS");
        var subTitle = new Rule(settingsType);

        AnsiConsole.Write(title);
        AnsiConsole.Write(subTitle);
        AnsiConsole.WriteLine(string.Empty);
    }

    private static void InitTable(Dictionary<int, SettingsKeyVal> dic, string title, bool ColumnMode = false)
    {
        var tableUI = new Spectre.Console.Table
        {
            Title = new TableTitle($"[green]{title}[/]")
        };

        tableUI.AddColumn("Settings");
        tableUI.AddColumn("Value");

        var section = string.Empty;

        // Add some rows
        foreach (var entry in dic)
        {
            if (section != entry.Value.Group)
            {
                tableUI.AddRow(string.Empty);
                tableUI.AddRow("---------" + entry.Value.Group + "-----------");
                section = entry.Value.Group;
            }

            if (ColumnMode)
                tableUI.AddRow(entry.Value.Label, entry.Value.Settings.ToString()!);
            else
            {
                if (!entry.Value.ColumnModeOnly)
                    tableUI.AddRow(entry.Value.Label, entry.Value.Settings.ToString()!);
            }
        }

        AnsiConsole.Write(tableUI);

    }

    private async Task<int> ManageUserInputForGlobalSettingsAsync()
    {
        //Manage user inputs
        AnsiConsole.WriteLine(string.Empty);
        var value = AnsiConsole.Ask<string>("Press the settings number you want to edit or (q) to return to main menu: ");

        if (value == "q")
            await ShowMainMenuAsync();


        if (!int.TryParse(value, out int intValue))
            await ShowGlobalSettingsAsync();

        return intValue;
    }

    private async Task<int> ManageUserInputForTableSettingsAsync()
    {
        //Manage user inputs
        AnsiConsole.WriteLine(string.Empty);

        var value = AnsiConsole.Ask<string>("Press the settings number you want to edit or (q) to return to main menu: ");

        if (value == "q")
            await ShowMainMenuAsync();


        if (!int.TryParse(value, out int intValue))
            await ShowTableSettingsAsync();

        return intValue;
    }

    private async Task<int> ManageUserInputForAdvancedSettingsAsync(SettingsTable advancedSettings, string tableName)
    {
        //Manage user inputs
        AnsiConsole.WriteLine(string.Empty);

        var value = AnsiConsole.Ask<string>(@"
!! Press the settings number you want to edit
or (c) to go down deeper in column mode (settings at column lvl)
or (r) to revert this table to global normal settings and go back main menu
or (q) to return to main menu");

        if (value == "q")
            await ShowMainMenuAsync();

        if (value == "c")
            await ShowAdvancedColumnsAsync(advancedSettings, tableName);

        if (value == "r")
        {
            try
            {
                _generatorService.GlobalGeneratorSettings.TableSettings.Remove(tableName);
                AnsiConsole.WriteLine("Table config reverted to global");
                await ReturnToMainMenuAsync();
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                AnsiConsole.WriteLine("Error to go back to global config");
                await ReturnToMainMenuAsync();
            }
        }

        if (!int.TryParse(value, out int intValue))
            await ShowTableSettingsAsync(true, advancedSettings, tableName);

        return intValue;
    }

    private async Task<int> ManageUserInputForAdvancedColSettingsAsync(SettingsColumn colAdvancedSettings, string tableName, string columnName)
    {
        //Manage user inputs
        AnsiConsole.WriteLine(string.Empty);

        var value = AnsiConsole.Ask<string>(@"
!! Press the settings number you want to edit
or (r) to revert this column to table column normal settings and go back to tabel advanced settings
or (q) to return to table advanced settings");

        if (value == "q")
            await ShowTableSettingsAsync(true, _generatorService.GlobalGeneratorSettings.TableSettings[tableName], tableName);

        if (value == "r")
        {
            try
            {
                _generatorService.GlobalGeneratorSettings.TableSettings[tableName].ColumnSettings.Remove(columnName);
                AnsiConsole.WriteLine("Column config reverted to table normal settings");
                AnsiConsole.WriteLine("Press any key to return to advanced table config");
                Console.ReadKey();
                await ShowTableSettingsAsync(true, _generatorService.GlobalGeneratorSettings.TableSettings[tableName], tableName);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                AnsiConsole.WriteLine("Error to go back to global config");
                await ReturnToMainMenuAsync();
            }
        }

        if (!int.TryParse(value, out int intValue))
            await ShowColumnSettingsAsync(colAdvancedSettings, tableName, columnName);

        return intValue;
    }

    private static void ChangeValue<T>(SettingsKeyVal setValue, object settings)
    {
        string newValue = setValue.Type == typeof(bool)
            ? AnsiConsole.Confirm(setValue.Label.Split(") ")[1]) ? "True" : "False"
                : AnsiConsole.Ask<string>("type (c) to clear" + Environment.NewLine + "or change "
                    + setValue.Label);

        if (setValue.Type == typeof(string) && newValue == "c")
            newValue = string.Empty;

        _ = (T)UISettingsHelper.SetSettingsStringValue(settings, setValue.PropertyName, newValue);
    }


}

