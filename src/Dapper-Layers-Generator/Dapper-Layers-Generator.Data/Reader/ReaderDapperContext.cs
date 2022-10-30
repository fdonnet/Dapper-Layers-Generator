﻿using Dapper;
using Microsoft.Extensions.Configuration;
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
        IDatabaseDefinitionsRepo DatabaseDefinitionsRepo { get; set; }
        void InitFluentMap();
    }

    public class ReaderDapperContext : IReaderDapperContext
    {
        protected readonly IConfiguration? _config;

        protected IDbConnection _cn = null!;
        protected string _schemas;

        protected bool _disposed = false;

        public IDbConnection Connection
        {
            get => _cn;
        }

        protected IDatabaseDefinitionsRepo _databaseDefinitionsRepo;
        public virtual IDatabaseDefinitionsRepo DatabaseDefinitionsRepo { get; set; } = default!;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ReaderDapperContext(IConfiguration config)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _config = config;
            _schemas = _config["DB:Schemas"];
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.Settings.CommandTimeout = 60000;
            InitFluentMap();

        }

        public virtual void InitFluentMap()
        {

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
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

}
