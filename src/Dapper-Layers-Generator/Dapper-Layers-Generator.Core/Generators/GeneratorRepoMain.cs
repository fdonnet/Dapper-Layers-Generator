using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoMain : IGeneratorFromTable
    {

    }
    public class GeneratorRepoMain : GeneratorFromTable, IGeneratorRepoMain
    {
        protected virtual string UsingDbProviderSpecific { get; init; } = string.Empty;
        protected virtual string DbProviderString { get; init; } = String.Empty;

        public GeneratorRepoMain(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter) : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            return string.IsNullOrEmpty(DbProviderString)
                ? $$"""
                    {{WriteUsingStatements()}}
                    {{WriteRepoHeaderAndConstructor()}}
                    {{WriteInterface()}}
                    {{WriteClass()}}
                    """
                : $$"""
                    {{WriteUsingStatements()}}
                    {{WriteRepoHeaderAndConstructor()}}
                    {{WriteClass()}}
                    """;
        }

        private string WriteRepoHeaderAndConstructor()
        {
            return string.IsNullOrEmpty(DbProviderString)
                ? $$"""
                    // =================================================================
                    // Repo class for table {{Table.Name}}
                    // Base abstract class that can be used with no specific db provider
                    // You can extend it via other partial files where you know that a 
                    // query can run the same on different db providers
                    // Author: {{_settings.AuthorName}}
                    // Repo name: {{ClassName}}RepoBase
                    // Generated: {{_settings.GenerationTimestamp:yyyy-MM-dd HH:mm:ss}} UTC
                    // WARNING: Never change this file manually (re-generate it)
                    // =================================================================
                
                    namespace {{_settings.TargetNamespaceForRepo}}
                    {
                    """
                : $$"""
                    // =================================================================
                    // Repo class for table {{Table.Name}}
                    // Specific repo implementation for dbprovider : {{DbProviderString}}
                    // You can extend it via other partial files where you have specific 
                    // queries for specific dbs (if standard SQL is not sufficent)
                    // Author: {{_settings.AuthorName}}
                    // Repo name: {{ClassName}}Repo{{DbProviderString}}
                    // Generated: {{_settings.GenerationTimestamp:yyyy-MM-dd HH:mm:ss}} UTC
                    // WARNING: Never change this file manually (re-generate it)
                    // =================================================================
                    
                    namespace {{_settings.TargetNamespaceForRepo}}
                    {
                    """;
        }

        private string WriteUsingStatements()
        {
            return
                $$"""
                using {{_settings.TargetNamespaceForPOCO}};
                using Dapper;
                using {{_settings.TargetNamespaceForDbContext}};{{(string.IsNullOrEmpty(UsingDbProviderSpecific)?string.Empty:Environment.NewLine+ UsingDbProviderSpecific)}}
                                                
                """;
        }

        private string WriteClass()
        {
            return string.IsNullOrEmpty(DbProviderString)
                ? $$"""
                   {{tab}}public abstract partial class {{ClassName}}RepoBase : I{{ClassName}}Repo
                   {{tab}}{
                   {{tab}}{{tab}}protected readonly I{{_settings.DbContextClassName}} _{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}};
                   {{tab}}{{tab}}
                   {{tab}}{{tab}}public {{ClassName}}RepoBase(I{{_settings.DbContextClassName}} {{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}})
                   {{tab}}{{tab}}{
                   {{tab}}{{tab}}{{tab}}_{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}} = {{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}};
                   {{tab}}{{tab}}}
                   

                   """
                : $$"""
                   {{tab}}public partial class {{ClassName}}Repo{{DbProviderString}} : {{ClassName}}RepoBase, I{{ClassName}}Repo
                   {{tab}}{
                   {{tab}}{{tab}}public {{ClassName}}Repo{{DbProviderString}}(I{{_settings.DbContextClassName}} {{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}): base ({{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}})
                   {{tab}}{{tab}}{
                   {{tab}}{{tab}}}
                   

                   """;
        }

        /// <summary>
        /// Only used in the repo main without specific provider
        /// </summary>
        /// <returns></returns>
        private string WriteInterface()
        {
            return
                $$"""
                {{tab}}/// =================================================================
                {{tab}}/// Author: {{_settings.AuthorName}}
                {{tab}}/// Description:	Interface for the repo {{ClassName}}Repo
                {{tab}}/// You can extend it with partial file
                {{tab}}/// ================================================================= 
                {{tab}}public partial interface I{{ClassName}}Repo
                {{tab}}{
                {{@WriteInterfaceMethods()}}
                {{tab}}}

                """;
        }

        private string WriteInterfaceMethods()
        {
            var output = new List<string>();

            if (TableSettings.GetAllGenerator)
            {
                output.Add($"{tab}{tab}Task<IEnumerable<{ClassName}>> GetAllAsync();");
            }

            if (TableSettings.GetByPkGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                output.Add($"{tab}{tab}Task<{ClassName}?> GetBy{GetPkMemberNamesString()}Async({GetPkMemberNamesStringAndType()});");
            }

            //ByPk list
            if (TableSettings.GetByPkListGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                output.Add($"{tab}{tab}Task<IEnumerable<{ClassName}>> GetBy{GetPkMemberNamesString()}Async({GetPkMemberNamesStringAndTypeList()});");
            }

            //By pk bulk
            if (TableSettings.GetByPkBulkGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                output.Add($"{tab}{tab}Task<IEnumerable<{ClassName}>> GetBy{GetPkMemberNamesString()}BulkAsync({GetPkMemberNamesStringAndTypeList()});");
            }

            //By unique indexes
            if (TableSettings.GetByUkGenerator && ColumnNamesByIndexNameDic.Any())
            {
                foreach (var index in ColumnNamesByIndexNameDic)
                {
                    output.Add($"{tab}{tab}Task<{ClassName}?> GetBy{GetUkMemberNamesString(index.Key)}Async({GetUkMemberNamesStringAndType(index.Key)});");
                }
            }

            //Add
            if (TableSettings.AddGenerator)
            {
                if (PkColumns.Count() == 1 && PkColumns.Where(c => c.IsAutoIncrement).Any())
                    output.Add($"{tab}{tab}Task<{GetPkMemberTypes()}> AddAsync({ClassName} " +
                            $"{_stringTransform.ApplyConfigTransformMember(ClassName)});");
                else
                    output.Add($"{tab}{tab}Task AddAsync({ClassName} " +
                    $"{_stringTransform.ApplyConfigTransformMember(ClassName)});");
            }

            //Add multi
            if (TableSettings.AddMultiGenerator)
            {
                output.Add($"{tab}{tab}Task AddAsync(IEnumerable<{ClassName}> " +
                $"{_stringTransform.PluralizeToLower(ClassName)});");
            }

            //Add bulk
            if (TableSettings.AddBulkGenerator)
            {
                output.Add($"{tab}{tab}Task AddBulkAsync(IEnumerable<{ClassName}> " +
                $"{_stringTransform.PluralizeToLower(ClassName)});");
            }

            //Update
            if (TableSettings.UpdateGenerator && ColumnForUpdateOperations!.Where(c => !c.IsAutoIncrement && !c.IsPrimary).Any())
            {
                output.Add($"{tab}{tab}Task UpdateAsync({ClassName} " +
                    $"{_stringTransform.ApplyConfigTransformMember(ClassName)});");
            }

            //Update multi
            if (TableSettings.UpdateMultiGenerator && ColumnForUpdateOperations!.Where(c => !c.IsAutoIncrement && !c.IsPrimary).Any())
            {
                output.Add($"{tab}{tab}Task UpdateAsync(IEnumerable<{ClassName}> " +
                $"{_stringTransform.PluralizeToLower(ClassName)});");
            }

            //Update bulk
            if (TableSettings.UpdateBulkGenerator && ColumnForUpdateOperations!.Where(c => !c.IsAutoIncrement && !c.IsPrimary).Any())
            {
                output.Add($"{tab}{tab}Task UpdateBulkAsync(IEnumerable<{ClassName}> " +
                $"{_stringTransform.PluralizeToLower(ClassName)});");
            }

            //Delete
            if (TableSettings.DeleteGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                output.Add($"{tab}{tab}Task DeleteAsync({GetPkMemberNamesStringAndType()});");
            }

            //Delete by pklist
            if (TableSettings.DeleteByPkListGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                output.Add($"{tab}{tab}Task DeleteAsync({GetPkMemberNamesStringAndTypeList()});");
            }

            //Delete bulk
            if (TableSettings.DeleteBulkGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                output.Add($"{tab}{tab}Task DeleteBulkAsync({GetPkMemberNamesStringAndTypeList()});");
            }

            return String.Join(Environment.NewLine,output);
        }
    }
}



