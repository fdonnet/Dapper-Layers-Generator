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
        public IList<ISchema>? SchemaDefinitions { get; private set; }

        private IReaderDapperContext _context;
        private IConfiguration _config;
        private IEnumerable<ITable>? _tables;
        private IEnumerable<IColumn>? _columns;
        private IEnumerable<IKey>? _keys;

        public ReaderDBDefinitionService(IConfiguration config, IReaderDapperContext context)
        {
            _config = config;
            _context = context; 
        }

        //Can manage para connection to dot it via DBContext Factory
        public async Task ReadAllDBDefinitionsStepAsync()
        {
            SchemaDefinitions = (await _context.DatabaseDefinitionsRepo.GetAllSchemasAsync()).ToList();
            _tables = await _context.DatabaseDefinitionsRepo.GetAllTablesAsync();
            _columns = await _context.DatabaseDefinitionsRepo.GetAllColumnsAsync();
            _keys = await _context.DatabaseDefinitionsRepo.GetAllPrimaryAndUniqueKeys();

            LinkDbDefinitions();
        }

        private void LinkDbDefinitions()
        {
            //Very bad loop => can do it better with linq
            if(SchemaDefinitions != null)
            {
                foreach (ISchema schema in SchemaDefinitions)
                {
                    schema.Tables = _tables?.Where(t => t.Schema == schema.Name).ToList();
                    
                    if (schema.Tables != null)
                    {
                        foreach (var table in schema.Tables)
                        {
                            table.Columns = _columns?.Where(c => c.Schema == schema.Name && c.Table == table.Name).ToList();

                            if(table.Columns != null)
                            {
                                foreach (var col in table.Columns)
                                {
                                    col.IsPrimary = _keys?.Where(k => k.Schema == col.Schema
                                                                && k.Table == col.Table
                                                                && k.Column == col.Name
                                                                && k.Type == KeyType.Primary).Any() ?? false;

                                    col.HasUniqueIndex = _keys?.Where(k => k.Schema == col.Schema
                                                                && k.Table == col.Table
                                                                && k.Column == col.Name
                                                                && k.Type == KeyType.Unique).Any() ?? false;

                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
