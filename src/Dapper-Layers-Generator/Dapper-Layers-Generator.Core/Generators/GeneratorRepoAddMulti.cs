using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{

    public interface IGeneratorRepoAddMulti : IGeneratorFromTable
    {

    }
    public class GeneratorRepoAddMulti : GeneratorForOperations, IGeneratorRepoAddMulti
    {

        public GeneratorRepoAddMulti(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.AddMultiGenerator)
            {
                return
                    $$"""
                    {{WriteMethodDef()}}
                    {{WriteOpenTransactionAndLoopBegin()}}
                    {{WriteDapperDynaParamsForInsert().Replace($"{tab}{tab}{tab}", $"{tab}{tab}{tab}{tab}")}}

                    {{WriteBaseSqlForInsert().Replace($"{tab}{tab}{tab}", $"{tab}{tab}{tab}{tab}")}}
                    {{WriteValuesToInsert().Replace($"{tab}{tab}{tab}", $"{tab}{tab}{tab}{tab}")}}

                    {{WriteDapperCall()}}

                    {{WriteCloseTransaction()}}
                    {{tab}}{{tab}}}

                    """;
            }

            return string.Empty;
        }

        protected override string WriteMethodDef()
        {
            return
                $$"""
                {{tab}}{{tab}}public {{(IsBase ? "virtual" : "override")}} async Task AddAsync(IEnumerable<{{ClassName}}> {{_stringTransform.PluralizeToLower(ClassName)}})
                {{tab}}{{tab}}{
                """;
        }

        protected override string WriteDapperCall()
        {
            var methodCall = $"{tab}{tab}{tab}{tab}_ = " +
            $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
            $"ExecuteAsync(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";

            return
                $$"""
                {{methodCall}}
                {{tab}}{{tab}}{{tab}}}
                """;
        }

        protected override string WriteReturnObj()
        {
            return string.Empty;
        }

    }
}
