using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsColumn
    {
        public bool StandardStringLengthDecorator { get; set; } = true;
        public bool StandardJsonIgnoreDecorator { get; set; } = false;
        public string? FieldNameCustomType { get; set; } = null;
        public string? FieldNameCustomDecorators { get; set; } = null;
    }
}
