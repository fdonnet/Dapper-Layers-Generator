using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Console.Helpers
{
    internal class SettingsKeyVal
    {
        internal string Label { get; set; } = string.Empty;
        internal string Settings { get; set; } = string.Empty;
        internal string PropertyName { get; set; } = string.Empty;
        internal string ChildOf { get; set; } = string.Empty;
        internal int Position { get; set; }
    }
}
