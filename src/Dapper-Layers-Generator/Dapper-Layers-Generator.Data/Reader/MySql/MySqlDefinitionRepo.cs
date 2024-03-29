﻿using Dapper;
using Dapper_Layers_Generator.Data.POCO;

namespace Dapper_Layers_Generator.Data.Reader.MySql
{

    /// <summary>
    /// Simple linq to MAP (auto mapper can do the job... tried fluentmap but have some limitations)
    /// </summary>
    public class MySqlDefinitionRepo : DatabaseDefinitionsRepoBase, IDatabaseDefinitionsRepo
    {
        public MySqlDefinitionRepo(IReaderDapperContext dbContext, string schemas) : base(dbContext, schemas)
        {
            
        }

        public async Task<IEnumerable<Schema>> GetAllSchemasAsync()
        {
            var p = new DynamicParameters();
            p.Add("@schemas", _sourceSchemas);

            var sql = @"SELECT schema_name
                        FROM schemata
                        WHERE schema_name in @schemas
                        ORDER by schema_name";

            var schemasDyna = await _dbContext.Connection.QueryAsync<dynamic>(sql, p);

            var schemas = schemasDyna.Select(s => new Schema()
            {
                Name = s.schema_name
            });

            return schemas;

        }

        public async Task<IEnumerable<Table>> GetAllTablesAsync()
        {
            var p = new DynamicParameters();
            p.Add("@schemas", _sourceSchemas);

            var sql = @"SELECT table_schema,table_name
                        FROM tables
                        WHERE table_schema in @schemas";

            var tablesDyna = await _dbContext.Connection.QueryAsync<dynamic>(sql,p);

            var tables = tablesDyna.Select(t => new Table()
            {
                Schema = t.table_schema,
                Name = t.table_name
            });

            return tables;

        }

        public async Task<IEnumerable<Column>> GetAllColumnsAsync()
        {
            var p = new DynamicParameters();
            p.Add("@schemas", _sourceSchemas);

            var sql = @"SELECT table_name
                        ,column_name
                        ,table_schema
                        ,ordinal_position
                        ,is_nullable
                        ,data_type
                        ,character_maximum_length
                        ,numeric_precision
                        ,numeric_scale
                        ,extra
                        ,column_type
                        FROM columns
                        WHERE table_schema in @schemas";

            var columnsDyna = await _dbContext.Connection.QueryAsync<dynamic>(sql, p);

            var columns = columnsDyna.Select(c => new Column()
            {
                Schema = c.table_schema,
                Table = c.table_name,
                Name = c.column_name,
                Position = (int)c.ordinal_position,
                IsNullable = c.is_nullable == "YES" ? true : false,
                DataType = c.data_type,
                Length = (int?)c.character_maximum_length ?? 0,
                Precision = (int?)c.numeric_precision ?? 0,
                Scale = (int?)c.numeric_scale ?? 0,
                IsAutoIncrement = c.extra == "auto_increment" ? true : false,
                CompleteType = c.column_type
            });

            return columns;
        }

        public async Task<IEnumerable<Key>> GetAllPrimaryAndUniqueKeysAsync()
        {
            var p = new DynamicParameters();
            p.Add("@schemas", _sourceSchemas);

            var sql = @"SELECT table_schema
                        ,table_name
                        ,column_name
                        ,index_name
                        FROM statistics
                        WHERE table_schema in @schemas
                        AND non_unique = 0";

            var constraintsDyna = await _dbContext.Connection.QueryAsync<dynamic>(sql, p);

            var keys = constraintsDyna.Select(c => new Key()
            {
                Schema = c.table_schema,
                Table = c.table_name,
                Column = c.column_name,
                Name = c.index_name,
                Type = c.index_name == "PRIMARY" ? KeyType.Primary : KeyType.Unique
            });

            return keys;
        }


    }
}
