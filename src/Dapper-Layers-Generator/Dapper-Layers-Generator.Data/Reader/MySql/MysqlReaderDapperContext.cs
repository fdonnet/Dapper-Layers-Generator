using Dapper.FluentMap;
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
                if (_databaseDefinitionsRepo == null)
                    _databaseDefinitionsRepo = new MySqlDefinitionRepo(this,_schemas);

                return _databaseDefinitionsRepo;
            }
        }

        public override void InitFluentMap()
        {
            FluentMapper.Initialize(config =>
            {
                config.AddMap<MySqlSchema>(new MySqlSchemaMap());
                config.AddMap<MySqlTable>(new MySqlTableMap());
                config.AddMap<MySqlColumn>(new MySqlColumnMap());
            });
        }
    }
}

