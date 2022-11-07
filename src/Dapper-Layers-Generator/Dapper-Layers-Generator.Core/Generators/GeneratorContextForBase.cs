using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorContextBase : IGenerator
    {

    }

    /// <summary>
    /// Generator : the base context used by all specific db providers.
    /// </summary>
    public class GeneratorContextForBase: GeneratorContextTemplate, IGeneratorContextBase
    {

        public GeneratorContextForBase(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService) 
            : base(settingsGlobal, data, stringTransformationService)
        {

        }

        public override string Generate()
        {
            var builder = new StringBuilder();
            //Header
            builder.Append(WriteContextHeaderComment());
            //Db context interface
            builder.Append(WriteInterface());
            //Db context class
            builder.Append(WriteFullClassContent());

            return builder.ToString();
        }

        protected override string WriteContextHeaderComment()
        {
            return $@"{@WriteUsingStatements()}
// =================================================================
// DBContext implements all repo management + a small context factory
//           this file contains interfaces declaration and abstract
//           base classes for factory and DB context
// Author: {_settings.AuthorName}
// Context name: {_settings.DbContextClassName}
// Generated: {DateTime.Now}
// WARNING: Never change this file manually (re-generate it)
// =================================================================

namespace {_settings.TargetNamespaceForDbContext}
{{";

        }

        protected override string WriteUsingStatements()
        {
            string output =$@"using System.Data;
using Microsoft.Extensions.Configuration;
using {_settings.TargetNamespaceForRepo};

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

        private string WriteInterfaceRepoMembers()
        {
            var tab = _stringTransform.IndentString;
            var membersDeclaration = String.Join(Environment.NewLine, _selectedTables!.Select(t =>
            {
                var tableName = _stringTransform.ApplyConfigTransformClass(t.Name);
                var settings = _settings.GetTableSettings(t.Name);

                var interfaceName = "I" + tableName + "Repo";
                var repoPropertyName = tableName + "Repo";

                return $"{tab}{tab}{interfaceName} {repoPropertyName} {{ get; }}";
            }));

            return membersDeclaration;
        }

        protected override string WriteClassRepoMembers()
        {
            var tab = _stringTransform.IndentString;
            var membersDeclaration = String.Join(Environment.NewLine, _selectedTables!.Select(t =>
            {
                var tableName = _stringTransform.ApplyConfigTransformClass(t.Name);
                var settings = _settings.GetTableSettings(t.Name);

                var interfaceName = "I" + tableName + "Repo";
                var repoClassName = tableName + "Repo";
                var repoProtectedFieldName = $"_{_stringTransform.FirstCharToLower(repoClassName)}";

                return $@"{tab}{tab}protected {interfaceName}? {repoProtectedFieldName};
{tab}{tab}public abstract {interfaceName} {repoClassName} {{ get; }}";
            }));

            return membersDeclaration;
        }

        protected override string WriteFullClassContent()
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
{tab}public abstract class {_settings.DbContextClassName}FactoryBase : {("I" + _settings.DbContextClassName)}Factory
{tab}{{
{tab}{tab}protected readonly IConfiguration _config;
{tab}{tab}
{tab}{tab}public DbContextFactoryBase(IConfiguration config)
{tab}{tab}{{
{tab}{tab}{tab}_config = config;
{tab}{tab}}}
{tab}{tab}
{tab}{tab}public abstract {("I" + _settings.DbContextClassName)} Create();
{tab}}}
{tab}
{tab}/// <summary>
{tab}/// Abstract DBContext base 
{tab}/// </summary>
{tab}public abstract class {_settings.DbContextClassName}Base : {("I" + _settings.DbContextClassName)}
{tab}{{
{tab}{tab}protected readonly IConfiguration _config;
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
{tab}
{tab}{tab}/// <summary>
{tab}{tab}/// Main constructor, inject standard config
{tab}{tab}/// </summary>
{tab}{tab}public {_settings.DbContextClassName}Base(IConfiguration config)
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
