using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Dapper_Layers_Generator.Data.Reader.MySql
{
    public class MysqlReaderDapperContext : ReaderDapperContext
    {
        public MysqlReaderDapperContext(IConfiguration config) : base(config)
        {
            _cn = new MySqlConnection(_config.GetConnectionString("Default"));
        }

        public override IDatabaseDefinitionsRepo DatabaseDefinitionsRepo
        {
            get
            {
                _databaseDefinitionsRepo ??= new MySqlDefinitionRepo(this,_schemas);

                return _databaseDefinitionsRepo;
            }
        }
    }
}

