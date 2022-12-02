using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoUpdateMulti : IGeneratorFromTable
    {

    }
    public class GeneratorRepoUpdateMulti : GeneratorForOperations, IGeneratorRepoUpdateMulti
    {

        public GeneratorRepoUpdateMulti(SettingsGlobal settingsGlobal
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
                var output = new StringBuilder();
                output.Append(WriteMethodDef());
                output.Append(Environment.NewLine);
                output.Append(WriteOpenTransactionAndLoopBegin());
                output.Append(Environment.NewLine);
                output.Append(GetDapperDynaParams());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(WriteBaseSqlForUpdate().Replace($"{tab}{tab}{tab}", $"{tab}{tab}{tab}{tab}"));
                output.Append(WriteSqlWhereClauseForPk().Replace($"{tab}{tab}{tab}", $"{tab}{tab}{tab}{tab}"));
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(WriteDapperCall());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(WriteCloseTransaction());
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}}}");
                output.Append(Environment.NewLine);

                return output.ToString();
            }

            return string.Empty;
        }

        protected override string WriteMethodDef()
        {
            return $"{tab}{tab}public {(IsBase ? "virtual" : "override")} async Task UpdateAsync(IEnumerable<{ClassName}> " +
            $"{_stringTransform.PluralizeToLower(ClassName)})" +
        @$"
{tab}{tab}{{";
        }

        protected override string WriteDapperCall()
        {
            return $"{tab}{tab}{tab}{tab}_ = " +
            $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
            $"ExecuteAsync(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);" +
            Environment.NewLine +
            $"{tab}{tab}{tab}}}";

        }

        protected virtual string GetDapperDynaParams()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}{tab}var p = new DynamicParameters();");
            output.Append(Environment.NewLine);

            var cols = ColumnForUpdateOperations!;

            var spParams = String.Join(Environment.NewLine, cols.OrderBy(c => c.Position).Select(col =>
            {
                return $@"{tab}{tab}{tab}{tab}p.Add(""@{col.Name}"", {_stringTransform.ApplyConfigTransformMember(ClassName)}.{_stringTransform.PascalCase(col.Name)});";
            }));

            output.Append(spParams);
            return output.ToString();
        }

        protected override string WriteReturnObj()
        {
            return string.Empty;
        }

    }
}
