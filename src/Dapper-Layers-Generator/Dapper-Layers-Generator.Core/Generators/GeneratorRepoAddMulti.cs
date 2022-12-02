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
                var output = new StringBuilder();
                output.Append(WriteMethodDef());
                output.Append(Environment.NewLine);
                output.Append(GetOpenTransactionAndLoopBegin());
                output.Append(Environment.NewLine);
                output.Append(GetDapperDynaParams());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(WriteBaseSqlForInsert().Replace($"{tab}{tab}{tab}", $"{tab}{tab}{tab}{tab}"));
                output.Append($"{tab}{tab}{tab}{tab}{tab}" + WriteValuesToInsert().Replace($"{tab}{tab}{tab}{tab}",$"{tab}{tab}{tab}{tab}{tab}"));
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
            return $"{tab}{tab}public {(IsBase ? "virtual" : "override")} async Task AddAsync(IEnumerable<{ClassName}> " +
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

        protected virtual string GetOpenTransactionAndLoopBegin()
        {
            return @$"{tab}{tab}{tab}var isTransAlreadyOpen = _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction != null;

{tab}{tab}{tab}if (!isTransAlreadyOpen)
{tab}{tab}{tab}{tab}await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.OpenTransactionAsync();

{tab}{tab}{tab}foreach(var {_stringTransform.ApplyConfigTransformMember(ClassName)} in {_stringTransform.PluralizeToLower(ClassName)})
{tab}{tab}{tab}{{";
        }

        protected virtual string GetDapperDynaParams()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}{tab}var p = new DynamicParameters();");
            output.Append(Environment.NewLine);

            var cols = ColumnForInsertOperations!.Where(c => !c.IsAutoIncrement);

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
