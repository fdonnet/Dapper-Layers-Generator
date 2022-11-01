﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsTable
    {
        //Can ignore certain columns names (separated by ,)
        public List<string> IgnoredColumnNames { get; set; } = new List<string>();

        //Global table settings 
        public SettingsColumn ColumnGlobalSettings { get; set; } = new SettingsColumn();

        //Override table global seetings
        public Dictionary<string, SettingsColumn> ColumnSettings { get; set; } = new Dictionary<string, SettingsColumn>();
    }
}
