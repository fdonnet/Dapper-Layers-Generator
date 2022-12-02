using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoUpdate : IGeneratorFromTable
    {

    }
    public class GeneratorRepoUpdate : GeneratorForOperations, IGeneratorRepoUpdate
    {
        public GeneratorRepoUpdate(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.UpdateGenerator && ColumnForUpdateOperations!.Where(c => !c.IsAutoIncrement && !c.IsPrimary).Any())
            {
                return
                    $$"""
                    {{WriteMethodDef()}}
                    {{WriteDapperDynaParams()}}

                    {{WriteBaseSqlForUpdate()}}
                    {{WriteSqlWhereClauseForPk()}}

                    {{WriteDapperCall()}}
                    {{tab}}{{tab}}}

                    """;
            }

            return string.Empty;
        }

        protected override string WriteMethodDef()
        {
            return
                $$"""
                {{tab}}{{tab}}public {{(IsBase ? "virtual" : "override")}} async Task UpdateAsync({{ClassName}} {{_stringTransform.ApplyConfigTransformMember(ClassName)}})
                {{tab}}{{tab}}{
                """;
        }

        protected override string WriteDapperCall()
        {
            return $"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected virtual string WriteDapperDynaParams()
        {
            var cols = ColumnForUpdateOperations!;

            var spParams = String.Join(Environment.NewLine, cols.OrderBy(c => c.Position).Select(col =>
            {
                return $@"{tab}{tab}{tab}p.Add(""@{col.Name}"", {_stringTransform.ApplyConfigTransformMember(ClassName)}.{_stringTransform.PascalCase(col.Name)});";
            }));

            return
                $$"""
                {{tab}}{{tab}}{{tab}}var p = new DynamicParameters();
                {{spParams}}
                """;
        }

        protected override string WriteReturnObj()
        {
            return $"{tab}{tab}{tab}return {ClassName.ToLower()};";
        }
    }
}
