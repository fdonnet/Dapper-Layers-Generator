using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Data.Reader
{
    public interface IDatabaseDefinitionsRepo
    {

    }

    public class DatabaseDefinitionsRepo : IDatabaseDefinitionsRepo
    {
        protected ReaderDapperContext _dbContext = null!;

        public DatabaseDefinitionsRepo(ReaderDapperContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
