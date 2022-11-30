using Dapper_Layers_Generator.Data.POCO;

namespace Dapper_Layers_Generator.Data.Reader
{
    public interface IDatabaseDefinitionsRepo
    {
        Task<IEnumerable<Schema>> GetAllSchemasAsync();
        Task<IEnumerable<Table>> GetAllTablesAsync();
        Task<IEnumerable<Column>> GetAllColumnsAsync(); //---- included auto-increment true or false (will see with other DBs if we need a separate method)
        Task<IEnumerable<Key>> GetAllPrimaryAndUniqueKeysAsync();
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
