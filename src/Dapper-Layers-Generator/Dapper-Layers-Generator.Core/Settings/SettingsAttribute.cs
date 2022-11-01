using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingsAttribute : Attribute
    {
        public string Message { get; set; } = string.Empty;
        public int Position { get; set; }
        public string ChildOf { get; set; } = string.Empty;
    }
}
