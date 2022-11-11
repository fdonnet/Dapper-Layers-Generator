namespace Dapper_Layers_Generator.Core.Settings
{
    //Position counter begin after Settings Table (don't forget to update)
    public class SettingsColumn
    {
        [SettingsAttribute(Message = "Enable Requiered decorator for not null fields: ", Position = 16)]
        public bool StandardRequiredDecorator { get; set; } = true;

        [SettingsAttribute(Message = "Enable StringLength decorator for string: ", Position = 17)]
        public bool StandardStringLengthDecorator { get; set; } = true;

        [SettingsAttribute(Message = "Fully qualified (full namespace) type (ex Enum): ", Position = 18, OnlyInColumnMode = true)]
        public string FieldNameCustomType { get; set; } = string.Empty;

        [SettingsAttribute(Message = "Fully qualified (full namespace) custom decorator: ", Position = 19, OnlyInColumnMode = true)]
        public string? FieldNameCustomDecorator { get; set; } = string.Empty;
    }
}
