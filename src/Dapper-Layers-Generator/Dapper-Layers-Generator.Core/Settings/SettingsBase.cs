using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Settings
{
    public class SettingsBase
    {
        public string AuthorName { get; set; } = "Dapper Layers Generator";
        public string OutputPath_POCOFile { get; set; } = "DapperLayersGenerator.POCO.cs";
        public string OutputPath_RepoFile { get; set; } = "DapperLayersGenerator.Repo.cs";
        public string Namespace_POCO { get; set; } = "DapperLayersGenerator.POCO";
        public string Namespace_Repo { get; set; } = "DapperLayersGenerator.Repo";

    }
}
