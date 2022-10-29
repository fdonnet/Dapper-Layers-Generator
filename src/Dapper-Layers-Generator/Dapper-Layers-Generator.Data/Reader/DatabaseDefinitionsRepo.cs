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
    }

    public class DatabaseDefinitionsRepoBase
    {
        protected ReaderDapperContext _dbContext = null!;
        protected string[] _schemas = default!;

        public DatabaseDefinitionsRepoBase(ReaderDapperContext dbContext, string[] schemas)
        {
            _dbContext = dbContext;
            _schemas = schemas;
        }
    }
}
