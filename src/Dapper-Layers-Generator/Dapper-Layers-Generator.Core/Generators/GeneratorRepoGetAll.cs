using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoGetAll : IGeneratorFromTable
    {

    }
    public class GeneratorRepoGetAll : GeneratorForOperations, IGeneratorRepoGetAll
    {
        public GeneratorRepoGetAll(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.GetAllGenerator)
            {
                return
                    $$""""
                    {{WriteMethodDef()}}
                    {{WriteBaseSqlForSelect()}}
                    {{tab}}{{tab}}{{tab}}""";

                    {{WriteDapperCall()}}

                    {{WriteReturnObj()}}
                    {{tab}}{{tab}}}

                    """";
            }
            return string.Empty;
        }

        protected override string WriteMethodDef()
        {
            return
                $$"""
                {{tab}}{{tab}}public {{(IsBase ? "virtual" : "override")}} async Task<IEnumerable<{{ClassName}}>> GetAllAsync()
                {{tab}}{{tab}}{
                """;
        }

        protected override string WriteDapperCall()
        {
            return  $"{tab}{tab}{tab}var {_stringTransform.PluralizeToLower(ClassName)} = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QueryAsync<{ClassName}>(sql,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected override string WriteReturnObj()
        {
            return $"{tab}{tab}{tab}return {_stringTransform.PluralizeToLower(ClassName)};";
        }

    }
}
