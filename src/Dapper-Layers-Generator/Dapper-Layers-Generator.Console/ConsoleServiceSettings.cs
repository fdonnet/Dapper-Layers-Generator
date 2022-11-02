using Dapper_Layers_Generator.Console.Helpers;
using Dapper_Layers_Generator.Core;
using Dapper_Layers_Generator.Core.Settings;
using MySqlX.XDevAPI.Relational;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Spectre.Console;
using System;
using System.Collections.Generic;
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

        InitTable(dic,"Config values");

        var intValue = await ManageUserInputForGlobalSettingsAsync();

        //Change settings values
        if (dic.ContainsKey(intValue))
        {
            //Schema 
            if (dic[intValue].PropertyName == "SelectedSchema")
            {
                AnsiConsole.WriteLine("");
                var newValue = AnsiConsole.Prompt(
                         new SelectionPrompt<string>()
                             .Title("Choose at least one schema to be generated: ")
                             .AddChoices(_config["DB:Schemas"].Split(",")));
            }
            else
            {
               ///(Need to be string type prop or boom) =with children props
                if (dic[intValue].PropertyName == "TargetProjectNamespace" || dic[intValue].PropertyName == "TargetProjectPath")
                {
                    var newValue = AnsiConsole.Ask<string>(dic[intValue].Label.Split(") ")[1]);
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
                }
                else // normal case
                {
                    ChangeValue<SettingsGlobal>(dic[intValue], globalSettings);
                }
            }
        }

        await ShowGlobalSettingsAsync();
    }

    internal async Task ShowTableSettingsAsync()
    {
        InitSettingsUI("Tables and columns global settings");

        //Global tab settings
        var tableSettings = _generatorService.GlobalGeneratorSettings.TableGlobalSettings;
        var dicTable = UISettingsHelper.SettingsDic(tableSettings);
        InitTable(dicTable,"Table settings");

        AnsiConsole.WriteLine(string.Empty);

        //Global col settings
        var colSettings = _generatorService.GlobalGeneratorSettings.TableGlobalSettings.ColumnGlobalSettings;
        var dicColumns = UISettingsHelper.SettingsDic(colSettings);
        InitTable(dicColumns, "Columns settings");

        
        var intValue = await ManageUserInputForTableSettingsAsync();

        //Change settings values
        if (dicTable.ContainsKey(intValue))
        {
            ChangeValue<SettingsTable>(dicTable[intValue], tableSettings);
        }
        else
        {
            if (dicColumns.ContainsKey(intValue))
            {
                ChangeValue<SettingsColumn>(dicColumns[intValue], colSettings);
            }
        }

        await ShowTableSettingsAsync();
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

    private static void InitTable(Dictionary<int, SettingsKeyVal> dic, string title, bool advancedColumnMode = false)
    {
        var tableUI = new Spectre.Console.Table();
        tableUI.Title = new TableTitle($"[green]{title}[/]");

        tableUI.AddColumn("Settings");
        tableUI.AddColumn("Value");

        var section = string.Empty;

        // Add some rows
        foreach (var entry in dic)
        {
            if(section != entry.Value.Group)
            {
                tableUI.AddRow(string.Empty);
                tableUI.AddRow("---------" + entry.Value.Group + "-----------");
                section = entry.Value.Group;
            }

            if(advancedColumnMode)
                tableUI.AddRow(entry.Value.Label, entry.Value.Settings.ToString()!);
            else
            {
                if(!entry.Value.AdvancedColumnMode)
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

    private void ChangeValue<T>(SettingsKeyVal setValue, object settings)
    {
        string newValue = string.Empty;

        if(setValue.Type == typeof(bool))
        {
            newValue = AnsiConsole.Confirm(setValue.Label.Split(") ")[1]) ? "True" : "False";
        }
        else
        {
            newValue = AnsiConsole.Ask<string>(setValue.Label.Split(") ")[1]);
        }

        _ = (T)UISettingsHelper.SetSettingsStringValue(settings, setValue.PropertyName, newValue);
    }


}

