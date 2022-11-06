using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorContext : IGenerator
    {

    }

    public abstract class GeneratorContext : GeneratorContextBase, IGeneratorContext
    {
        protected abstract string UsingDbProviderSpecific { get; init; }
        protected abstract string DbProviderString { get; init; }
        protected abstract string ConnectionStringInject { get; init; }
        protected abstract string DapperDefaultMapStrat { get; init; }
        protected abstract string DapperCommandTimeOut { get; init; }
        protected abstract string ConnectionClassName { get; init; }

        public GeneratorContext(SettingsGlobal settingsGlobal
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

            //Db context class
            builder.Append(WriteClass());

            return builder.ToString();

        }

        private string WriteContextHeaderComment()
        {
            return $@"{@WriteUsingStatements()}
// =================================================================
// DBContext implements all repo management + a small context factory
// Inherits from DbContext base abstract class
// Specific for DB provider {DbProviderString}
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
            string output = $@"using {_settings.TargetNamespaceForRepo};
using System.Data;
{UsingDbProviderSpecific}
using Dapper;
using Microsoft.Extensions.Configuration;

";

            return output;
        }

        private string WriteClass()
        {
            return WriteFullClassContent();
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

                return $@"{tab}{tab}public override {interfaceName} {repoClassName} 
{tab}{tab}{{
{tab}{tab}{tab}get {{
{tab}{tab}{tab}{tab}{repoProtectedFieldName} ??= new {repoClassName}(this);
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
{tab}/// Used when the DBcontext itself is not suffisent to manage its lifecycle 
{tab}/// Factory specific for the dbprovider {DbProviderString}
{tab}/// Inherits from factory base
{tab}/// </summary>
{tab}public class {_settings.DbContextClassName}{DbProviderString}Factory :  {_settings.DbContextClassName}FactoryBase, {("I" + _settings.DbContextClassName)}Factory
{tab}{{
{tab}{tab}public {_settings.DbContextClassName}{DbProviderString}Factory(IConfiguration config) : base (config)
{tab}{tab}{{
{tab}{tab}}}
{tab}{tab}
{tab}{tab}public override I{_settings.DbContextClassName} Create()
{tab}{tab}{{
{tab}{tab}{tab}return new {_settings.DbContextClassName}{DbProviderString}(_config);
{tab}{tab}}}
{tab}}}
{tab}
{tab}/// <summary>
{tab}/// Used when the DBcontext
{tab}/// Specific for the dbprovider {DbProviderString}
{tab}/// Inherits from {_settings.DbContextClassName}Base
{tab}/// </summary>
{tab}public class {_settings.DbContextClassName}{DbProviderString} : {_settings.DbContextClassName}Base,  {("I" + _settings.DbContextClassName)}
{tab}{{
{tab}{tab}public override IDbConnection Connection {{get;init;}}
{tab}{tab}
{@WriteClassRepoMembers()}
{tab}{tab}/// <summary>
{tab}{tab}/// Main constructor, inject standard config : Default connection string
{tab}{tab}/// </summary>
{tab}{tab}public {_settings.DbContextClassName}{DbProviderString}(IConfiguration config) : base (config)
{tab}{tab}{{
{tab}{tab}{tab}{DapperDefaultMapStrat}
{tab}{tab}{tab}{DapperCommandTimeOut}
{tab}{tab}{tab}{ConnectionStringInject}
{tab}{tab}}}
{tab}{tab}
{tab}{tab}/// <summary>
{tab}{tab}/// Open a transaction with a specified isolation level
{tab}{tab}/// </summary>
{tab}{tab}public override async Task<IDbTransaction> OpenTransactionAsync(IsolationLevel? level = null)
{tab}{tab}{{
{tab}{tab}{tab}if(_trans != null)
{tab}{tab}{tab}{tab}throw new Exception(""A transaction is already open, you need to use a new {_settings.DbContextClassName} for parallel job."");
{tab}{tab}{tab}
{tab}{tab}{tab}if (Connection.State == ConnectionState.Closed)
{tab}{tab}{tab}{{
{tab}{tab}{tab}{tab}await (({ConnectionClassName})Connection).OpenAsync();
{tab}{tab}{tab}}}
{tab}{tab}{tab}
{tab}{tab}{tab}_trans = level == null ? Connection.BeginTransaction() : Connection.BeginTransaction((IsolationLevel)level);
{tab}{tab}{tab}
{tab}{tab}{tab}return _trans;
{tab}{tab}}}
{tab}}}
}}
";


        }
    }
}
