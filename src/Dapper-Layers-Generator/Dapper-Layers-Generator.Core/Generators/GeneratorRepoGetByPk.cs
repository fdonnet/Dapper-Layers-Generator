using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoGetByPk : IGeneratorFromTable
    {

    }

    public class GeneratorRepoGetByPk : GeneratorForOperations, IGeneratorRepoGetByPk
    {
        public GeneratorRepoGetByPk(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.GetByPkGenerator)
            {
                if (!PkColumns.Any())
                    throw new ArgumentException($"You cannot run the Get by Pk Generator for table {Table.Name}, no pk defined");

                return
                    $$"""
                    {{WriteMethodDef()}}
                    {{WriteDapperDynaParamsForPk()}}

                    {{WriteBaseSqlForSelect()}}
                    {{WriteSqlWhereClauseForPk()}}

                    {{WriteDapperCall()}}

                    {{WriteReturnObj()}}
                    {{tab}}{{tab}}}

                    """;
            }
            return string.Empty;
        }

        protected override string WriteMethodDef()
        {
            return
                $$"""
                {{tab}}{{tab}}public {{(IsBase ? "virtual" : "override")}} async Task<{{ClassName}}?> GetBy{{GetPkMemberNamesString()}}Async({{GetPkMemberNamesStringAndType()}})
                {{tab}}{{tab}}{
                """;
        }

        protected override string WriteDapperCall()
        {
            return $"{tab}{tab}{tab}var {ClassName.ToLower()} = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QuerySingleOrDefaultAsync<{ClassName}>(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected override string WriteReturnObj()
        {
            return $"{tab}{tab}{tab}return {ClassName.ToLower()};";
        }

    }
}
