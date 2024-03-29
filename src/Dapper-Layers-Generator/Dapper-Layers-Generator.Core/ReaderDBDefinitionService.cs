﻿using Dapper_Layers_Generator.Data.POCO;
using Dapper_Layers_Generator.Data.Reader;

namespace Dapper_Layers_Generator.Core
{
    public interface IReaderDBDefinitionService
    {
        IList<Schema>? SchemaDefinitions { get; }
        Task ReadAllDBDefinitionsStepAsync();
    }

    public class ReaderDBDefinitionService : IReaderDBDefinitionService
    {
        public IList<Schema>? SchemaDefinitions { get; private set; }

        private readonly IReaderDapperContext _context;
        private IEnumerable<Table>? _tables;
        private IEnumerable<Column>? _columns;
        private IEnumerable<Key>? _keys;

        public ReaderDBDefinitionService(IReaderDapperContext context)
        {
            _context = context; 
        }

        //Can manage para connection to dot it via DBContext Factory
        public async Task ReadAllDBDefinitionsStepAsync()
        {
            SchemaDefinitions = (await _context.DatabaseDefinitionsRepo.GetAllSchemasAsync()).ToList();
            _tables = await _context.DatabaseDefinitionsRepo.GetAllTablesAsync();
            _columns = await _context.DatabaseDefinitionsRepo.GetAllColumnsAsync();
            _keys = await _context.DatabaseDefinitionsRepo.GetAllPrimaryAndUniqueKeysAsync();

            LinkDbDefinitions();
        }

        private void LinkDbDefinitions()
        {
            //Very bad loop => can do it better with linq
            if(SchemaDefinitions != null)
            {
                foreach (Schema schema in SchemaDefinitions)
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

                                    col.UniqueIndexNames = _keys?.Where(k => k.Schema == col.Schema
                                                                && k.Table == col.Table
                                                                && k.Column == col.Name
                                                                && k.Type == KeyType.Unique).Select(k=>k.Name).ToList();

                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
