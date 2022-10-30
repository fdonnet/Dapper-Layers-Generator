using Dapper_Layers_Generator.Data.POCO;
using Dapper_Layers_Generator.Data.Reader;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core
{
    public class ReaderDBDefinitionService
    {
        public IEnumerable<ITable>? TablesDefinitions { get; private set; }

        private IReaderDapperContext _context;
        private IConfiguration _config;
        
        public ReaderDBDefinitionService(IConfiguration config, IReaderDapperContext context)
        {
            _config = config;
            _context = context; 
        }

        //Can manage para connection to dot it via DBContext Factory
        public async Task ReadAllDBDefinitionsStepAsync()
        {
            await ReadTableDefinitionsAsync();
            await ReadColumnDefinitionsAsync();
        }

        private async Task ReadTableDefinitionsAsync()
        {
            TablesDefinitions = await _context.DatabaseDefinitionsRepo.GetAllTablesAsync();
        }

        private async Task ReadColumnDefinitionsAsync()
        {
            var _columnDefintions = await _context.DatabaseDefinitionsRepo.GetAllColumnsAsync();

            //Link columns to tables (better way to do that with linq, no time)
            if(TablesDefinitions != null)
            {
                foreach (var table in TablesDefinitions)
                {
                    table.Columns = _columnDefintions.Where(c => c.Table == table.Name);
                }
            }
        }
    }
}
