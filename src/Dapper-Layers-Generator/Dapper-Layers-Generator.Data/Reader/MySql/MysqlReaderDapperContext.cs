using Dapper_Layers_Generator.Data.POCO.MySql;
using Dapper_Layers_Generator.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

