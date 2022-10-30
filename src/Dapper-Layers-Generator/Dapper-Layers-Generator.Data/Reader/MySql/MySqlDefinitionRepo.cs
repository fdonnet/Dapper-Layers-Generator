using Dapper;
using Dapper.FluentMap;
using Dapper_Layers_Generator.Data.POCO;
using Dapper_Layers_Generator.Data.POCO.MySql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Data.Reader.MySql
{
    public class MySqlDefinitionRepo : DatabaseDefinitionsRepoBase, IDatabaseDefinitionsRepo
    {
        public MySqlDefinitionRepo(IReaderDapperContext dbContext, string schemas) : base(dbContext, schemas)
        {
            
        }

        public async Task<IEnumerable<ITable>> GetAllTablesAsync()
        {
            var p = new DynamicParameters();
            p.Add("@schemas", _sourceSchemas);

            var sql = @"SELECT table_schema,table_name
                        FROM tables
                        WHERE table_schema in @schemas
                        ORDER by table_schema, table_name";

            var tables = await _dbContext.Connection.QueryAsync<MySqlTable>(sql,p);

            return tables;

        }
    }
}
