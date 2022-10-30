using Dapper_Layers_Generator.Data.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Data.Reader
{
    public interface IDatabaseDefinitionsRepo
    {
        Task<IEnumerable<ITable>> GetAllTablesAsync();
        Task<IEnumerable<IColumn>> GetAllColumnsAsync();
    }

    public class DatabaseDefinitionsRepoBase
    {
        protected IReaderDapperContext _dbContext = null!;
        protected string _schemas = string.Empty;
        protected string[] _sourceSchemas;

        public DatabaseDefinitionsRepoBase(IReaderDapperContext dbContext, string schemas)
        {
            _dbContext = dbContext;
            _schemas = schemas;
            _sourceSchemas = _schemas.Split(",");
        }
    }
}
