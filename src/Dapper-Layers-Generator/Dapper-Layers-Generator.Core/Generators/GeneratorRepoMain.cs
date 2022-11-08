using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var output = new StringBuilder();
            output.Append(WriteRepoHeaderAndConstructor());

            if (string.IsNullOrEmpty(DbProviderString))
                output.Append(WriteInterface());

            output.Append(WriteClass());

            return output.ToString();
        }

        private string WriteRepoHeaderAndConstructor()
        {
            return string.IsNullOrEmpty(DbProviderString)
                ? $@"{@WriteUsingStatements()}
// =================================================================
// Repo class for table {Table.Name}
// Base abstract class that can be used with no specific db provider
// You can extend it via other partial files where you know that a 
// query can run the same on different db providers
// Author: {_settings.AuthorName}
// Repo name: {ClassName}RepoBase
// Generated: {DateTime.Now}
// WARNING: Never change this file manually (re-generate it)
// =================================================================

namespace {_settings.TargetNamespaceForRepo}
{{"
                : $@"{@WriteUsingStatements()}
// =================================================================
// Repo class for table {Table.Name}
// Specific repo implementation for dbprovider : {DbProviderString}
// You can extend it via other partial files where you have specific 
// queries for specific dbs (if standard SQL is not sufficent)
// Author: {_settings.AuthorName}
// Repo name: {ClassName}Repo{DbProviderString}
// Generated: {DateTime.Now}
// WARNING: Never change this file manually (re-generate it)
// =================================================================

namespace {_settings.TargetNamespaceForRepo}
{{";

        }

        private string WriteUsingStatements()
        {
            string output = $@"using {_settings.TargetNamespaceForPOCO};
using Dapper;
using {_settings.TargetNamespaceForDbContext};
{UsingDbProviderSpecific}

";
            return output;
        }

        private string WriteClass()
        {

            return string.IsNullOrEmpty(DbProviderString)
                ? @$"
{tab}public abstract partial class {ClassName}RepoBase : I{ClassName}Repo
{tab}{{
{tab}{tab}protected readonly I{_settings.DbContextClassName} _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)};
{tab}{tab}
{tab}{tab}public {ClassName}RepoBase(I{_settings.DbContextClassName} {_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)})
{tab}{tab}{{
{tab}{tab}{tab}_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)} = {_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)};
{tab}{tab}}}

"
                : @$"
{tab}public partial class {ClassName}Repo{DbProviderString} : {ClassName}RepoBase, I{ClassName}Repo
{tab}{{
{tab}{tab}public {ClassName}Repo{DbProviderString}(I{_settings.DbContextClassName} {_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}): base ({_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)})
{tab}{tab}{{
{tab}{tab}}}

";

        }

        /// <summary>
        /// Only used in the repo main without specific provider
        /// </summary>
        /// <returns></returns>
        private string WriteInterface()
        {
            return $@" 
{tab}/// =================================================================
{tab}/// Author: {_settings.AuthorName}
{tab}/// Description:	Interface for the repo {ClassName}Repo
{tab}/// You can extend it with partial file
{tab}/// ================================================================= 
{tab}public partial interface I{ClassName}Repo
{tab}{{
{@WriteInterfaceMethods()}
{tab}}}";

        }

        private string WriteInterfaceMethods()
        {
            var output = new StringBuilder();

            if (TableSettings.GetAllGenerator)
            {
                output.Append($"{tab}{tab}Task<IEnumerable<{ClassName}>> GetAllAsync();");
                output.Append(Environment.NewLine);
            }

            if (TableSettings.GetByPkGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                output.Append($"{tab}{tab}Task<{ClassName}> GetBy{GetPkMemberNamesString()}Async({GetPkMemberNamesStringAndType()});");
                output.Append(Environment.NewLine);
            }

            //Will see if we can find a solution for PkList if composite PKs
            if (TableSettings.GetByPkListGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                output.Append($"{tab}{tab}Task<IEnumerable<{ClassName}>> GetByListOf{GetPkMemberNamesString()}Async({GetPkMemberNamesStringAndTypeList()});");
                output.Append(Environment.NewLine);
            }

            //By unique indexes
            if(TableSettings.GetByUkGenerator && ColumnNamesByIndexNameDic.Any())
            {
                foreach(var index in ColumnNamesByIndexNameDic)
                {
                    output.Append($"{tab}{tab}Task<{ClassName}> GetBy{GetUkMemberNamesString(index.Key)}Async({GetUkMemberNamesStringAndType(index.Key)});");
                    output.Append(Environment.NewLine);
                }
            }

            //Add
            if(TableSettings.AddGenerator)
            {
                output.Append($"{tab}{tab}Task<{GetPkMemberTypes()}> AddAsync({_stringTransform.ApplyConfigTransformClass(ClassName)} " +
                    $"{_stringTransform.ApplyConfigTransformMember(ClassName)});");

                output.Append(Environment.NewLine);
            }
            
            //Update
            if (TableSettings.UpdateGenerator && ColumnForUpdateOperations!.Where(c => !c.IsAutoIncrement && !c.IsPrimary).Any())
            {
                output.Append($"{tab}{tab}Task UpdateAsync({_stringTransform.ApplyConfigTransformClass(ClassName)} " +
                    $"{_stringTransform.ApplyConfigTransformMember(ClassName)});");

                output.Append(Environment.NewLine);
            }

            //Delete
            if (TableSettings.DeleteGenerator)
            {
                output.Append($"{tab}{tab}Task DeleteAsync({GetPkMemberNamesStringAndType()});");

                output.Append(Environment.NewLine);
            }

            return output.ToString();
        }
    }
}



