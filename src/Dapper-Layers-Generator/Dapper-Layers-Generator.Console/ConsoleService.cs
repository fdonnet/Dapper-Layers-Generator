using Dapper_Layers_Generator.Core;
using Dapper_Layers_Generator.Data.Reader;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace Dapper_Layers_Generator.Console
{
    public class ConsoleService
    {
        private readonly IConfiguration _config;
        private readonly ReaderDBDefinitionService _reader;

        public ConsoleService(IConfiguration config, ReaderDBDefinitionService reader)
        {
            _config = config;
            _reader = reader;   
        }

        public string BeginSummaryPrint(string dbProvider)
        {
            var schmeas = _config["DB:Schemas"];
            var msg =

@$"Hello, based on your config injection you will run Dapper Layers Generator on:
DBPROVIDER : {dbProvider}

and for this source DB schemas: 
SCHEMAS : {schmeas}";

            return msg;

        }

        public async Task<string> LoadDBDefinitionsAsync()
        {
            var msg = string.Empty;
            try
            {
                await _reader.ReadAllDBDefinitionsStepAsync();

                msg =
$@"SUCCESS to READ your DB.
Would you like to print all the definitions (Y)es or (N)o ?";

            }
            catch(Exception ex)
            {
                msg =
$@"Error to READ your DB definitions.
Program will shut down.

ERROR: {ex.Message}

TRACE: {ex.StackTrace}";
            }

            return msg;
        }

        public string PrintDBDefintionAsync()
        {
            var option = new JsonSerializerOptions() { WriteIndented = true };
            var jsonExtract = JsonSerializer.Serialize(_reader.TablesDefinitions,option);
            var msg =
$@"JSON extract of your table defintions:

{jsonExtract}";

            return msg;

        }
    }
}
