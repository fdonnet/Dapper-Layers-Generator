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
                var output = new StringBuilder();
                output.Append(GetMethodDef());

                if (PkColumns.Count() == 1 || !IsBase)
                {
                    output.Append(Environment.NewLine);
                    output.Append(GetDapperDynaParamsForPkList());
                    output.Append(Environment.NewLine);
                    output.Append(Environment.NewLine);
                    output.Append(@GetBaseSqlForSelect());
                    output.Append(Environment.NewLine);
                    output.Append(GetSqlPkListWhereClause());
                    output.Append(Environment.NewLine);
                    output.Append(Environment.NewLine);
                    output.Append(GetDapperCall());
                    output.Append(Environment.NewLine);
                    output.Append(Environment.NewLine);
                    output.Append(GetReturnObj());
                    output.Append(Environment.NewLine);
                    output.Append($"{tab}{tab}}}");
                }

                output.Append(Environment.NewLine);
                return output.ToString();
            }

            return string.Empty;
        }

        protected override string GetMethodDef()
        {
            return PkColumns.Count() > 1
                ? $"{tab}{tab}public {(IsBase ? "abstract" : "override async")} " +
                        $"Task<IEnumerable<{ClassName}>> GetBy{GetPkMemberNamesString()}Async({GetPkMemberNamesStringAndTypeList()}){(IsBase ? ";" : String.Empty)}"
                : $"{tab}{tab}public {(IsBase ? "virtual" : "override")} " +
                $"async Task<IEnumerable<{ClassName}>> GetBy{GetPkMemberNamesString()}Async({GetPkMemberNamesStringAndTypeList()})" +
                @$"
{tab}{tab}{{";
        }

        protected override string GetDapperCall()
        {
            return $"{tab}{tab}{tab}var {_stringTransform.PluralizeToLower(ClassName)} = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QueryAsync<{ClassName}>(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected override string GetReturnObj()
        {
            return $"{tab}{tab}{tab}return {_stringTransform.PluralizeToLower(ClassName)};";
        }

    }
}
