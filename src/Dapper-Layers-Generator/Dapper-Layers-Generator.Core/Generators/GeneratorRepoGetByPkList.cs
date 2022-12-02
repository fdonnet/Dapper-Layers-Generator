using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoGetByPkList : IGeneratorFromTable
    {

    }
    public class GeneratorRepoGetByPkList : GeneratorForOperations, IGeneratorRepoGetByPkList
    {
        public GeneratorRepoGetByPkList(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }
        public override string Generate()
        {
            if (TableSettings.GetByPkListGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                var output = string.Empty;

                if (PkColumns.Count() == 1 || !IsBase)
                {
                    output =
                        $$"""

                        {{WriteDapperDynaParamsForPkList()}}

                        {{WriteBaseSqlForSelect()}}
                        {{WriteSqlPkListWhereClause()}}

                        {{WriteDapperCall()}}

                        {{WriteReturnObj()}}
                        {{tab}}{{tab}}}
                        """;
                }
                return
                    $$"""
                    {{WriteMethodDef()}}{{output}}

                    """;
            }
            return string.Empty;
        }

        protected override string WriteMethodDef()
        {
            if (PkColumns.Count() > 1)
                return
                    $$"""
                    {{tab}}{{tab}}public {{(IsBase ? "abstract" : "override async")}} Task<IEnumerable<{{ClassName}}>> GetBy{{GetPkMemberNamesString()}}Async({{GetPkMemberNamesStringAndTypeList()}}){{(IsBase ? ";" : String.Empty)}}
                    """;
            else
                return
                    $$"""
                    {{tab}}{{tab}}public {{(IsBase ? "virtual" : "override")}} async Task<IEnumerable<{{ClassName}}>> GetBy{{GetPkMemberNamesString()}}Async({{GetPkMemberNamesStringAndTypeList()}})
                    {{tab}}{{tab}}{
                    """;
        }

        protected override string WriteDapperCall()
        {
            return $"{tab}{tab}{tab}var {_stringTransform.PluralizeToLower(ClassName)} = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QueryAsync<{ClassName}>(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected override string WriteReturnObj()
        {
            return $"{tab}{tab}{tab}return {_stringTransform.PluralizeToLower(ClassName)};";
        }

    }
}
