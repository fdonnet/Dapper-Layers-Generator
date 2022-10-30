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
        private IReaderDapperContext _context;
        private IConfiguration _config;

        public ReaderDBDefinitionService(IConfiguration config, IReaderDapperContext context)
        {
            _config = config;
            _context = context; 
        }

        public async Task ReadAllDBDefinitionsStepAsync()
        {
            await ReadTableDefinitionsAsync();
        }

        private async Task ReadTableDefinitionsAsync()
        {
            var tables = await _context.DatabaseDefinitionsRepo.GetAllTablesAsync();
        }
    }
}
