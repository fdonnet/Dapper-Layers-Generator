using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Settings
{
    //Position counter begin after Settings Table (don't forget to update)
    public class SettingsColumn
    {
        [SettingsAttribute(Message = "Enable Requiered decorator for not null fields: ", Position = 11)]
        public bool StandardRequiredDecorator { get; set; } = true;
        
        [SettingsAttribute(Message = "Enable StringLength decorator for string: ", Position = 12)]
        public bool StandardStringLengthDecorator { get; set; } = true;

        [SettingsAttribute(Message = "Enable JSON ignore decorator: ", Position = 13, OnlyInColumnMode =true)]
        public bool StandardJsonIgnoreDecorator { get; set; } = false;
        
        [SettingsAttribute(Message = "Fully qualified (full namespace) type: ", Position = 14, OnlyInColumnMode = true)]
        public string FieldNameCustomType { get; set; } = string.Empty;
        
        [SettingsAttribute(Message = "Fully qualified (full namespace) custom decorator: ", Position = 15, OnlyInColumnMode = true)]
        public string? FieldNameCustomDecorators { get; set; } = string.Empty;

        [SettingsAttribute(Message = "Ignore this column: ", Position = 16, OnlyInColumnMode = true)]
        public bool Ignore { get; set; } = false;
    }
}
