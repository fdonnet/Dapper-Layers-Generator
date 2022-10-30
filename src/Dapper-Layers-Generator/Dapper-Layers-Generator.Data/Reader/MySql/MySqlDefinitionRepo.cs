using Dapper;
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
        public MySqlDefinitionRepo(ReaderDapperContext dbContext, string schemas) : base(dbContext, schemas)
        {
            
        }

        public async Task<IEnumerable<ITable>> GetAllTablesAsync()
        {
            var p = new DynamicParameters();
            p.Add("@shemas", _sourceSchemas);

            var sql = @"SELECT table_schema,table_name
                        FROM tables
                        WHERE table_schema in @schemas";

            var tables = await _dbContext.Connection.QueryAsync<MySqlTable>(sql, p);

            return tables;

        }
    }
}
