//using Dapper_Layers_Generator.Core.Converters;
//using Dapper_Layers_Generator.Core.Settings;
//using Dapper_Layers_Generator.Data.POCO;
//using System.Text;
//using System.Threading.Tasks;

//namespace Dapper_Layers_Generator.Core.Generators
//{
//    public interface IGeneratorContext : IGenerator
//    {

//    }

//    public abstract class GeneratorContextBase : Generator, IGeneratorContext
//    {
//        private IEnumerable<ITable>? _selectedTables;
//        protected abstract string UsingDbProviderSpecific { get; init; }
//        protected abstract string ConnectionStringInject { get; init; }
//        protected abstract string ConnectionStringSimple { get; init; }
//        protected abstract string DapperDefaultMapStrat { get; init; }
//        protected abstract string DapperCommandTimeOut { get; init; }

//        public GeneratorContextBase(SettingsGlobal settingsGlobal
//            , IReaderDBDefinitionService data
//            , StringTransformationService stringTransformationService)
//            : base(settingsGlobal, data, stringTransformationService)
//        {

//        }


//        public override string Generate()
//        {
//            if (_currentSchema.Tables == null)
//                throw new NullReferenceException("Cannot generate without at least one table selected");
//            _selectedTables = _currentSchema.Tables.Where(t => GetSelectedTableNames().Contains(t.Name));

//            var builder = new StringBuilder();
//            //Header
//            builder.Append(WriteContextHeaderComment());
//            //Db context interface
//            builder.Append(WriteInterface());
//            //Db context class
//            builder.Append(WriteClass());



//            return builder.ToString();

//        }

//        private string WriteContextHeaderComment()
//        {
//            return $@"{@WriteUsingStatements()}
//// =================================================================
//// DBContext implements all repo management + a small context factory
//// Author: {_settings.AuthorName}
//// Context name: {_settings.DbContextClassName}
//// Generated: {DateTime.Now}
//// WARNING: Never change this file manually (re-generate it)
//// =================================================================

//namespace {_settings.TargetNamespaceForDbContext}
//{{";

//        }

//        private string WriteUsingStatements()
//        {
//            string output = $@"using System;
//using {_settings.TargetNamespaceForRepo};
//using System.Data;
//using System.Data.Common;
//{UsingDbProviderSpecific}
//using Dapper;
//using Microsoft.Extensions.Configuration;

//";

//            return output;
//        }

//        private string WriteInterface()
//        {
//            var tab = _stringTransform.IndentString;
//            var interfaceName = "I" + _settings.DbContextClassName;
//            var builder = new StringBuilder();

//            builder.Append(Environment.NewLine);
//            builder.Append($"{tab}public interface {interfaceName} : IDisposable");
//            builder.Append(Environment.NewLine);
//            builder.Append($"{tab}{{");
//            builder.Append(Environment.NewLine);
//            builder.Append($@"{tab}{tab}IDbConnection Connection {{ get; }}
//{tab}{tab}IDbTransaction Transaction {{ get; }}

//{tab}{tab}Task<IDbTransaction> OpenTransaction();
//{tab}{tab}Task<IDbTransaction> OpenTransaction(IsolationLevel level);
//{tab}{tab}void CommitTransaction(bool disposeTrans = true);
//{tab}{tab}void RollbackTransaction(bool disposeTrans = true);");
//            builder.Append(Environment.NewLine);
//            builder.Append(Environment.NewLine);
//            builder.Append(WriteInterfaceRepoMembers());
//            builder.Append(Environment.NewLine);
//            builder.Append($"{tab}}}");
//            builder.Append(Environment.NewLine);

//            return builder.ToString();

//        }

//        private string WriteClass()
//        {
//            return WriteFullClassContent();
//        }


//        private string WriteInterfaceRepoMembers()
//        {
//            var tab = _stringTransform.IndentString;
//            var membersDeclaration = String.Join(Environment.NewLine, _selectedTables!.Select(t =>
//            {
//                var tableName = _stringTransform.ApplyConfigTransform(t.Name);
//                var settings = _settings.GetTableSettings(t.Name);

//                var interfaceName = "I" + tableName + "Repo";
//                var repoPropertyName = tableName + "Repo";

//                return $"{tab}{tab}{interfaceName} {repoPropertyName} {{ get; }}";
//            }));

//            return membersDeclaration;
//        }

//        private string WriteClassRepoMembers()
//        {
//            var tab = _stringTransform.IndentString;
//            var membersDeclaration = String.Join(Environment.NewLine, _selectedTables!.Select(t =>
//            {
//                var tableName = _stringTransform.ApplyConfigTransform(t.Name);
//                var settings = _settings.GetTableSettings(t.Name);

//                var interfaceName = "I" + tableName + "Repo";
//                var repoClassName = tableName + "Repo";
//                var repoProtectedFieldName = $"_{_stringTransform.FirstCharToLower(repoClassName)}";

//                return $@"{tab}{tab}protected {interfaceName} {repoProtectedFieldName};
//{tab}{tab}public {interfaceName} {repoClassName} {{
//{tab}{tab}{tab}get {{
//{tab}{tab}{tab}{tab}if ({repoProtectedFieldName} == null) 
//{tab}{tab}{tab}{tab}{tab}{repoProtectedFieldName} = new {repoClassName}(this);
//{tab}{tab}{tab}{tab}return {repoProtectedFieldName};
//{tab}{tab}{tab}}}
//{tab}{tab}}}
//";
//            }));

//            return membersDeclaration;
//        }

//        private string WriteFullClassContent()
//        {
//            var tab = _stringTransform.IndentString;

//            return $@"

//{tab}/// <summary>
//{tab}///Interface for {_settings.DbContextClassName}Factory
//{tab}/// </summary>
//{tab}public interface {("I" + _settings.DbContextClassName)}Factory
//{tab}{{
//{tab}    {_settings.DbContextClassName} Create();
//{tab}}}
//{tab}
//{tab}/// <summary>
//{tab}/// Used when the DBcontext itself is not suffisent to manage its lifecycle 
//{tab}/// Ex in WPF app you need to dispose the DBContexts to allow connection pooling and to be thread safe
//{tab}/// Very simple implementation = with only one DB connection (can be extended to support multiple DB con)
//{tab}/// </summary>
//{tab}public class {_settings.DbContextClassName}Factory : {("I" + _settings.DbContextClassName)}Factory
//{tab}{{
//{tab}{tab}protected readonly string _conString;
//{tab}{tab}protected readonly IConfiguration _config;
//{tab}{tab}
//{tab}{tab}public DbContextFactory(IConfiguration config)
//{tab}{tab}{{
//{tab}{tab}{tab}_config = config;
//{tab}{tab}{tab}_conString = _config.GetConnectionString(""Default"");
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}public {_settings.DbContextClassName}Factory(string dbConnectionString)
//{tab}{tab}{{
//{tab}{tab}{tab}_conString = dbConnectionString;
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}public {_settings.DbContextClassName} Create()
//{tab}{tab}{{
//{tab}{tab}{tab}return new {_settings.DbContextClassName}(_conString);
//{tab}{tab}}}
//{tab}}}
//{tab}
//{tab}public class {_settings.DbContextClassName} : {("I" + _settings.DbContextClassName)}
//{tab}{{
//{tab}{tab}protected readonly IConfiguration _config;
//{tab}{tab}
//{tab}{tab}
//{tab}{tab}protected IDbConnection _cn = null;
//{tab}{tab}public IDbConnection Connection {{
//{tab}{tab}    get => _cn;
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}
//{tab}{tab}protected IDbTransaction _trans = null;
//{tab}{tab}public IDbTransaction Transaction {{ 
//{tab}{tab}    get => _trans;
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}private bool _disposed = false;
//{tab}{tab}
//{tab}{tab}
//{@WriteClassRepoMembers()}
//{tab}{tab}/// <summary>
//{tab}{tab}/// Main constructor, inject standard config : Default connection string
//{tab}{tab}/// Need to be reviewed to be more generic (choose the connection string to inject)
//{tab}{tab}/// </summary>
//{tab}{tab}public {_settings.DbContextClassName}(IConfiguration config)
//{tab}{tab}{{
//{tab}{tab}{tab}_config = config;
//{tab}{tab}{tab}{DapperDefaultMapStrat}
//{tab}{tab}{tab}{DapperCommandTimeOut}
//{tab}{tab}{tab}{ConnectionStringInject}
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}/// <summary>
//{tab}{tab}/// Main constructor, inject standard config : Default connection string
//{tab}{tab}/// Pass the connection string directly (in case of usage with WPF or dekstop app, can be heavy to always inject)
//{tab}{tab}/// </summary>
//{tab}{tab}public {_settings.DbContextClassName}(string connectionString)
//{tab}{tab}{{
//{tab}{tab}{tab}{DapperDefaultMapStrat}
//{tab}{tab}{tab}{DapperCommandTimeOut}
//{tab}{tab}{tab}{ConnectionStringSimple}
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}/// <summary>
//{tab}{tab}/// Open a transaction
//{tab}{tab}/// </summary>
//{tab}{tab}public async Task<IDbTransaction> OpenTransaction()
//{tab}{tab}{{
//{tab}{tab}{tab}if(_trans != null)
//{tab}{tab}{tab}{tab}throw new Exception(""A transaction is already open, you need to use a new {_settings.DbContextClassName} for parallel job."");
//{tab}{tab}{tab}
//{tab}{tab}{tab}if (_cn.State == ConnectionState.Closed)
//{tab}{tab}{tab}{{
//{tab}{tab}{tab}{tab}if (!(_cn is DbConnection))
//{tab}{tab}{tab}{tab}{tab}throw new Exception(""Connection object does not support OpenAsync."");
//{tab}{tab}{tab}{tab}
//{tab}{tab}{tab}{tab}await (_cn as DbConnection).OpenAsync();
//{tab}{tab}{tab}}}
//{tab}{tab}{tab}
//{tab}{tab}{tab}_trans = _cn.BeginTransaction();
//{tab}{tab}{tab}
//{tab}{tab}{tab}return _trans;
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}/// <summary>
//{tab}{tab}/// Open a transaction with a specified isolation level
//{tab}{tab}/// </summary>
//{tab}{tab}public async Task<IDbTransaction> OpenTransaction(IsolationLevel level)
//{tab}{tab}{{
//{tab}{tab}{tab}if(_trans != null)
//{tab}{tab}{tab}{tab}throw new Exception(""A transaction is already open, you need to use a new {_settings.DbContextClassName} for parallel job."");
//{tab}{tab}{tab}
//{tab}{tab}{tab}if (_cn.State == ConnectionState.Closed)
//{tab}{tab}{tab}{{
//{tab}{tab}{tab}{tab}if (!(_cn is DbConnection))
//{tab}{tab}{tab}{tab}{tab}throw new Exception(""Connection object does not support OpenAsync."");
//{tab}{tab}{tab}{tab}
//{tab}{tab}{tab}{tab}await (_cn as DbConnection).OpenAsync();
//{tab}{tab}{tab}}}
//{tab}{tab}{tab}
//{tab}{tab}{tab}_trans = _cn.BeginTransaction(level);
//{tab}{tab}{tab}
//{tab}{tab}{tab}return _trans;
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}/// <summary>
//{tab}{tab}/// Commit the current transaction, and optionally dispose all resources related to the transaction.
//{tab}{tab}/// </summary>
//{tab}{tab}public void CommitTransaction(bool disposeTrans = true)
//{tab}{tab}{{
//{tab}{tab}{tab}if  (_trans == null)
//{tab}{tab}{tab}{tab}throw new Exception(""DB Transaction is not present."");
//{tab}{tab}{tab}
//{tab}{tab}{tab}_trans.Commit();
//{tab}{tab}{tab}if (disposeTrans) _trans.Dispose();
//{tab}{tab}{tab}if (disposeTrans) _trans = null;
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}/// <summary>
//{tab}{tab}/// Rollback the transaction and all the operations linked to it, and optionally dispose all resources related to the transaction.
//{tab}{tab}/// </summary>
//{tab}{tab}public void RollbackTransaction(bool disposeTrans = true)
//{tab}{tab}{{
//{tab}{tab}{tab}if  (_trans == null)
//{tab}{tab}{tab}{tab}throw new Exception(""DB Transaction is not present."");
//{tab}{tab}{tab}
//{tab}{tab}{tab}_trans.Rollback();
//{tab}{tab}{tab}if (disposeTrans) _trans.Dispose();
//{tab}{tab}{tab}if (disposeTrans) _trans = null;
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}/// <summary>
//{tab}{tab}/// Will be call at the end of the service (ex : transient service in api net core) GC correct way
//{tab}{tab}/// </summary>
//{tab}{tab}public void Dispose()
//{tab}{tab}{{
//{tab}{tab}{tab}Dispose(true);
//{tab}{tab}{tab}GC.SuppressFinalize(this);
//{tab}{tab}}}
//{tab}{tab}
//{tab}{tab}/// <summary>
//{tab}{tab}/// Better way to dispose if someone needs to inherit the DB context and have to dispose unmanaged ressources
//{tab}{tab}/// </summary>
//{tab}{tab}private void Dispose(bool disposing)
//{tab}{tab}{{
//{tab}{tab}{tab}if(! _disposed)
//{tab}{tab}{tab}{{
//{tab}{tab}{tab}{tab}if(disposing)
//{tab}{tab}{tab}{tab}{{
//{tab}{tab}{tab}{tab}{tab}_trans?.Dispose();
//{tab}{tab}{tab}{tab}{tab}_cn?.Close();
//{tab}{tab}{tab}{tab}{tab}_cn?.Dispose();
//{tab}{tab}{tab}{tab}}}
//{tab}{tab}{tab}}}
//{tab}{tab}{tab}_disposed = true;             
//{tab}{tab}}}
//{tab}}}
//}}
//";


//        }
//    }
//}
