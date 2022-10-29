using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Data.Reader
{

    public interface IReaderDapperContext : IDisposable
    {

        IDbConnection Connection { get; }
        IDatabaseDefinitionsRepo DatabaseDefinitionsRepo { get; }

    }
    
    public interface IDbReaderDapperContextFactory
    {
        IReaderDapperContext Create();
    }

    public class ReaderDapperContext : IReaderDapperContext
    {
        protected readonly IConfiguration? _config;

        protected IDbConnection _cn = null!;
        private bool _disposed = false;


        public IDbConnection Connection
        {
            get => _cn;
        }

        protected IDatabaseDefinitionsRepo? _databaseDefinitionsRepo;
        public IDatabaseDefinitionsRepo DatabaseDefinitionsRepo
        {
            get
            {
                _databaseDefinitionsRepo ??= new DatabaseDefinitionsRepo(this);

                return _databaseDefinitionsRepo;
            }
        }

        public ReaderDapperContext(IConfiguration config)
        {
            _config = config;
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.Settings.CommandTimeout = 60000;
            _cn = new MySqlConnection(_config.GetConnectionString("Default"));
        }

        public ReaderDapperContext(string connectionString)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.Settings.CommandTimeout = 60000;
            _cn = new MySqlConnection(connectionString);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cn?.Close();
                    _cn?.Dispose();
                }
            }
            _disposed = true;
        }

    }

    public class DbReaderDapperContextFactory : IDbReaderDapperContextFactory
    {
        protected readonly string _conStr;
        protected readonly IConfiguration _config = null!;

        public DbReaderDapperContextFactory(IConfiguration config)
        {
            _config = config;
            _conStr = _config.GetConnectionString("Default");
        }

        public IReaderDapperContext Create()
        {
            return new ReaderDapperContext(_conStr);
        }

    }
}
