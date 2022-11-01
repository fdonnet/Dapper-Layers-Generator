using Dapper_Layers_Generator.Console.Helpers;
using Dapper_Layers_Generator.Core;
using Dapper_Layers_Generator.Core.Settings;
using MySqlX.XDevAPI.Relational;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

internal partial class ConsoleService
{
    internal async Task ShowSettingsAsync()
    {
        InitSettingsUI();

        var globalSettings = _generatorService.GlobalGeneratorSettings;

        //Set first schema from config
        globalSettings.SelectedSchema = string.IsNullOrEmpty(_generatorService.GlobalGeneratorSettings.SelectedSchema)
             ? globalSettings.SelectedSchema = _config["DB:Schemas"].Split(",")[0]
             : globalSettings.SelectedSchema;

        //Init settings div
        var dic = UISettingsHelper.SettingsDic(globalSettings);

        int intValue = await InitTableAndWaitUserChoice(dic);

        //Change settings values
        if (dic.ContainsKey(intValue))
        {
            string newValue = string.Empty;
            if (dic[intValue].PropertyName == "SelectedSchema")
            {
                AnsiConsole.WriteLine("");
                newValue = AnsiConsole.Prompt(
                         new SelectionPrompt<string>()
                             .Title("Choose at least one schema to be generated: ")
                             .AddChoices(_config["DB:Schemas"].Split(",")));
            }
            else
            {
                newValue = AnsiConsole.Ask<string>(dic[intValue].Label.Split(") ")[1]);

                if (dic[intValue].PropertyName == "TargetProjectNamespace" || dic[intValue].PropertyName == "TargetProjectPath")
                {
                    if (AnsiConsole.Confirm("Do you want to try to update child namespaces"))
                    {
                        var tmpDic = dic.Where(d => d.Value.ChildOf == dic[intValue].PropertyName);
                        foreach(var item in tmpDic)
                        {
                            var oldChildValue = item.Value.Settings;
                            if(oldChildValue.Contains(dic[intValue].Settings))
                            {
                                var newChildValue = oldChildValue.Replace(dic[intValue].Settings, newValue);
                                globalSettings = UISettingsHelper.SetGlobalSettingsStringValue(globalSettings, item.Value.PropertyName, newChildValue);
                            }
                        }
                    }
                }
            }

            globalSettings = UISettingsHelper.SetGlobalSettingsStringValue(globalSettings, dic[intValue].PropertyName, newValue);
        }

        await ShowSettingsAsync();
    }

    private void InitSettingsUI()
    {
        AnsiConsole.Clear();
        string extract = string.Empty;
        AnsiConsole.WriteLine("Choose the value you want to change:");
    }

    private async Task<int> InitTableAndWaitUserChoice(Dictionary<int,SettingsKeyVal>dic)
    {
        var table = new Spectre.Console.Table();

        table.AddColumn("Settings");
        table.AddColumn("Value");

        // Add some rows
        foreach (var entry in dic)
        {
            table.AddRow(entry.Value.Label, entry.Value.Settings);
        }

        AnsiConsole.Write(table);

        //Manage user inputs
        var value = AnsiConsole.Ask<string>("Press the settings number you want to edit or (q) to return to main menu: ");

        if (value == "q")
            await ShowMainMenuAsync();

        int intValue;

        if (!int.TryParse(value, out intValue))
            await ShowSettingsAsync();

        return intValue;
    }


}

