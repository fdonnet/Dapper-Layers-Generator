using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoDeleteByPkList : IGeneratorFromTable
    {

    }
    public class GeneratorRepoDeleteByPkList : GeneratorForOperations, IGeneratorRepoDeleteByPkList
    {
        public GeneratorRepoDeleteByPkList(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.DeleteByPkListGenerator)
            {
                if (!PkColumns.Any())
                    throw new ArgumentException($"You cannot run the Delete by PkList Generator for table {Table.Name}, no pk defined");

                var output = string.Empty;

                if (PkColumns.Count() == 1 || !IsBase)
                {
                    output =
                        $$"""
                        {{WriteDapperDynaParamsForPkList()}}

                        {{WriteBaseSqlForDelete()}}
                        {{WriteSqlPkListWhereClause()}}

                        {{WriteDapperCall()}}
                        {{tab}}{{tab}}}

                        """;
                }
                return
                    $$"""
                    {{WriteMethodDef()}}
                    {{output}}
                    """;
            }
            return string.Empty;
        }

        protected override string WriteMethodDef()
        {
            if (PkColumns.Count() > 1)
                return
                    $$"""
                    {{tab}}{{tab}}public {{(IsBase ? "abstract" : "override async")}} Task DeleteAsync({{GetPkMemberNamesStringAndTypeList()}}){{(IsBase ? ";" : String.Empty)}}
                    """;
            else
                return
                    $$"""
                    {{tab}}{{tab}}public {{(IsBase ? "virtual" : "override")}} async Task DeleteAsync({{GetPkMemberNamesStringAndTypeList()}})
                    {{tab}}{{tab}}{
                    """;
        }

        protected override string WriteDapperCall()
        {
            return $"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected override string WriteReturnObj()
        {
            return string.Empty;
        }
    }
}
