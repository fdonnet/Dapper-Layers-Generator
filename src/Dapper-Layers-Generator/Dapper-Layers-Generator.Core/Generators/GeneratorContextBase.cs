using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorContext : IGenerator
    {

    }

    public abstract class GeneratorContextBase: Generator, IGeneratorContext
    {
        private IEnumerable<ITable>? _selectedTables;
        protected abstract string UsingDbProviderSpecific { get; init; }
        protected abstract string ConnectionStringInject { get; init; }
        protected abstract string ConnectionStringSimple { get; init; }
        protected abstract string DapperDefaultMapStrat { get; init; }
        protected abstract string DapperCommandTimeOut { get; init; }

        public GeneratorContextBase(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService) 
            : base(settingsGlobal, data, stringTransformationService)
        {

        }


        public override string Generate()
        {
            if (_currentSchema.Tables == null)
                throw new NullReferenceException("Cannot generate without at least one table selected");
            _selectedTables = _currentSchema.Tables.Where(t => GetSelectedTableNames().Contains(t.Name));

            var builder = new StringBuilder();
            //Header
            builder.Append(WriteContextHeaderComment());
            //Db context interface
            builder.Append(WriteInterface());
            //Db context class
            builder.Append(WriteClass());



            return builder.ToString();

        }

        private string WriteContextHeaderComment()
        {
            return $@"{@WriteUsingStatements()}
// =================================================================
// DBContext implements all repo management + a small context factory
// Author: {_settings.AuthorName}
// Context name: {_settings.DbContextClassName}
// Generated: {DateTime.Now}
// WARNING: Never change this file manually (re-generate it)
// =================================================================

namespace {_settings.TargetNamespaceForDbContext}
{{";

        }

        private string WriteUsingStatements()
        {
            string output =$@"using System;
using {_settings.TargetNamespaceForRepo};
using System.Data;
using System.Data.Common;
{UsingDbProviderSpecific}
using Dapper;
using Microsoft.Extensions.Configuration;

";

            return output;
        }

        private string WriteInterface()
        {
            var tab = _stringTransform.IndentString;
            var interfaceName = "I" + _settings.DbContextClassName;
            var builder = new StringBuilder();

            builder.Append(Environment.NewLine);
            builder.Append($"{tab}public interface {interfaceName} : IDisposable");
            builder.Append(Environment.NewLine);
            builder.Append($"{tab}{{");
            builder.Append(Environment.NewLine);
            builder.Append($@"{tab}{tab}IDbConnection Connection {{ get; }}
{tab}{tab}IDbTransaction? Transaction {{ get; }}

{tab}{tab}Task<IDbTransaction> OpenTransactionAsync(IsolationLevel? level);
{tab}{tab}void CommitTransaction(bool disposeTrans = true);
{tab}{tab}void RollbackTransaction(bool disposeTrans = true);");
            builder.Append(Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append(WriteInterfaceRepoMembers());
            builder.Append(Environment.NewLine);
            builder.Append($"{tab}}}");
            builder.Append(Environment.NewLine);

            return builder.ToString();

        }

        private string WriteClass()
        {
            return WriteFullClassContent();
        }


        private string WriteInterfaceRepoMembers()
        {
            var tab = _stringTransform.IndentString;
            var membersDeclaration = String.Join(Environment.NewLine, _selectedTables!.Select(t =>
            {
                var tableName = _stringTransform.ApplyConfigTransform(t.Name);
                var settings = _settings.GetTableSettings(t.Name);

                var interfaceName = "I" + tableName + "Repo";
                var repoPropertyName = tableName + "Repo";

                return $"{tab}{tab}{interfaceName} {repoPropertyName} {{ get; }}";
            }));

            return membersDeclaration;
        }

        private string WriteClassRepoMembers()
        {
            var tab = _stringTransform.IndentString;
            var membersDeclaration = String.Join(Environment.NewLine, _selectedTables!.Select(t =>
            {
                var tableName = _stringTransform.ApplyConfigTransform(t.Name);
                var settings = _settings.GetTableSettings(t.Name);

                var interfaceName = "I" + tableName + "Repo";
                var repoClassName = tableName + "Repo";
                var repoProtectedFieldName = $"_{_stringTransform.FirstCharToLower(repoClassName)}";

                return $@"{tab}{tab}protected {interfaceName} {repoProtectedFieldName};
{tab}{tab}public {interfaceName} {repoClassName} {{
{tab}{tab}{tab}get {{
{tab}{tab}{tab}{tab}if ({repoProtectedFieldName} == null) 
{tab}{tab}{tab}{tab}{tab}{repoProtectedFieldName} = new {repoClassName}(this);
{tab}{tab}{tab}{tab}return {repoProtectedFieldName};
{tab}{tab}{tab}}}
{tab}{tab}}}
";
            }));

            return membersDeclaration;
        }

        private string WriteFullClassContent()
        {
            var tab = _stringTransform.IndentString;

            return $@"

{tab}/// <summary>
{tab}///Interface for {_settings.DbContextClassName}Factory
{tab}/// </summary>
{tab}public interface {("I" + _settings.DbContextClassName)}Factory
{tab}{{
{tab}    {("I" + _settings.DbContextClassName)} Create();
{tab}}}
{tab}
{tab}/// <summary>
{tab}/// Used when the DBcontext itself is not suffisent to manage its lifecycle 
{tab}/// Very simple implementation
{tab}/// </summary>
{tab}public abstract class {_settings.DbContextClassName}Factory : {("I" + _settings.DbContextClassName)}Factory
{tab}{{
{tab}{tab}protected readonly IConfiguration _config;
{tab}{tab}
{tab}{tab}public DbContextFactory(IConfiguration config)
{tab}{tab}{{
{tab}{tab}{tab}_config = config;
{tab}{tab}}}
{tab}{tab}
{tab}{tab}public abstract {("I" + _settings.DbContextClassName)} Create();
{tab}}}
{tab}
{tab}public abstract class {_settings.DbContextClassName} : {("I" + _settings.DbContextClassName)}
{tab}{{
{tab}{tab}protected readonly IConfiguration _config;
{tab}{tab}
{tab}{tab}public abstract IDbConnection Connection {{get;init;}}
{tab}{tab}
{tab}{tab}protected IDbTransaction? _trans = null;
{tab}{tab}public IDbTransaction? Transaction {{ 
{tab}{tab}    get => _trans;
{tab}{tab}}}
{tab}{tab}
{tab}{tab}private bool _disposed = false;
{tab}{tab}
{@WriteClassRepoMembers()}
{tab}{tab}/// <summary>
{tab}{tab}/// Main constructor, inject standard config
{tab}{tab}/// </summary>
{tab}{tab}public {_settings.DbContextClassName}(IConfiguration config)
{tab}{tab}{{
{tab}{tab}{tab}_config = config;
{tab}{tab}}}
{tab}{tab}
{tab}{tab}/// <summary>
{tab}{tab}/// Open a transaction with a specified isolation level
{tab}{tab}/// </summary>
{tab}{tab}public abstract Task<IDbTransaction> OpenTransactionAsync(IsolationLevel? level);
{tab}{tab}
{tab}{tab}/// <summary>
{tab}{tab}/// Commit the current transaction, and optionally dispose all resources related to the transaction.
{tab}{tab}/// </summary>
{tab}{tab}public void CommitTransaction(bool disposeTrans = true)
{tab}{tab}{{
{tab}{tab}{tab}if  (_trans == null)
{tab}{tab}{tab}{tab}throw new NullReferenceException(""DB Transaction is not present."");
{tab}{tab}{tab}
{tab}{tab}{tab}_trans.Commit();
{tab}{tab}{tab}if (disposeTrans) _trans.Dispose();
{tab}{tab}{tab}if (disposeTrans) _trans = null;
{tab}{tab}}}
{tab}{tab}
{tab}{tab}/// <summary>
{tab}{tab}/// Rollback the transaction and all the operations linked to it, and optionally dispose all resources related to the transaction.
{tab}{tab}/// </summary>
{tab}{tab}public void RollbackTransaction(bool disposeTrans = true)
{tab}{tab}{{
{tab}{tab}{tab}if  (_trans == null)
{tab}{tab}{tab}{tab}throw new NullReferenceException(""DB Transaction is not present."");
{tab}{tab}{tab}
{tab}{tab}{tab}_trans.Rollback();
{tab}{tab}{tab}if (disposeTrans) _trans.Dispose();
{tab}{tab}{tab}if (disposeTrans) _trans = null;
{tab}{tab}}}
{tab}{tab}
{tab}{tab}/// <summary>
{tab}{tab}/// Will be call at the end of the service (ex : transient service in api net core) GC correct way
{tab}{tab}/// </summary>
{tab}{tab}public void Dispose()
{tab}{tab}{{
{tab}{tab}{tab}Dispose(true);
{tab}{tab}{tab}GC.SuppressFinalize(this);
{tab}{tab}}}
{tab}{tab}
{tab}{tab}/// <summary>
{tab}{tab}/// Better way to dispose if someone needs to inherit the DB context and have to dispose unmanaged ressources
{tab}{tab}/// </summary>
{tab}{tab}private void Dispose(bool disposing)
{tab}{tab}{{
{tab}{tab}{tab}if(! _disposed)
{tab}{tab}{tab}{{
{tab}{tab}{tab}{tab}if(disposing)
{tab}{tab}{tab}{tab}{{
{tab}{tab}{tab}{tab}{tab}_trans?.Dispose();
{tab}{tab}{tab}{tab}{tab}Connection.Close();
{tab}{tab}{tab}{tab}{tab}Connection.Dispose();
{tab}{tab}{tab}{tab}}}
{tab}{tab}{tab}}}
{tab}{tab}{tab}_disposed = true;             
{tab}{tab}}}
{tab}}}
}}
";


        }
    }
}
