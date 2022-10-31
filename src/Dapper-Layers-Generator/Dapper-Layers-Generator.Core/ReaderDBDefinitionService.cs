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
        public IEnumerable<ISchema>? SchemaDefinitions { get; private set; }

        private IReaderDapperContext _context;
        private IConfiguration _config;
        private IEnumerable<ITable>? _tables;
        private IEnumerable<IColumn>? _columns;

        public ReaderDBDefinitionService(IConfiguration config, IReaderDapperContext context)
        {
            _config = config;
            _context = context; 
        }

        //Can manage para connection to dot it via DBContext Factory
        public async Task ReadAllDBDefinitionsStepAsync()
        {
            SchemaDefinitions = await _context.DatabaseDefinitionsRepo.GetAllSchemasAsync();
            _tables = await _context.DatabaseDefinitionsRepo.GetAllTablesAsync();
            _columns = await _context.DatabaseDefinitionsRepo.GetAllColumnsAsync();

            LinkDbDefinitions();
        }

        private void LinkDbDefinitions()
        {
            //Very bad loop => can do it better with linq
            if(SchemaDefinitions != null)
            {
                foreach (var schema in SchemaDefinitions)
                {
                    schema.Tables = _tables?.Where(t => t.Schema == schema.Name);
                    
                    if (schema.Tables != null)
                    {
                        foreach (var table in schema.Tables)
                        {
                            table.Columns = _columns?.Where(c => c.Schema == schema.Name && c.Table == table.Name);
                        }
                    }
                }
            }
        }
    }
}
