namespace Dapper_Layers_Generator.Core.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingsAttribute : Attribute
    {
        public string Message { get; set; } = string.Empty;
        public int Position { get; set; }
        public string ChildOf { get; set; } = string.Empty;
        public bool OnlyInColumnMode { get; set; } = false;
        public string Group { get; set; } = string.Empty;
        public bool IsColumnListChoice { get; set; } = false;

    }
}
