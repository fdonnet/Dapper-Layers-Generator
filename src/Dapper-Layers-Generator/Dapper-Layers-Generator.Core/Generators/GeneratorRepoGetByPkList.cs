using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (TableSettings.GetByPkListGenerator)
            {
                var output = new StringBuilder();
                output.Append(GetMethodDef());

                if (PkColumns.Count() == 1 || !IsBase)
                {
                    output.Append(Environment.NewLine);
                    output.Append(GetDapperDynaParams());
                    output.Append(Environment.NewLine);
                    output.Append(Environment.NewLine);
                    output.Append(@GetBaseSqlForSelect());
                    output.Append(Environment.NewLine);
                    output.Append(GetSqlWhereClause());
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
                ? $"{tab}{tab}public {(IsBase ? "abstract" : "override")} " +
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

        protected virtual string GetSqlWhereClause()
        {
            var output = new StringBuilder();

            output.Append($"{tab}{tab}{tab}WHERE ");
            output.Append($"{ColAndTableIdentifier}{PkColumns.First().Name}{ColAndTableIdentifier} IN @listOf");

            output.Append("\";");
            return output.ToString();

        }

        protected override string GetReturnObj()
        {
            return $"{tab}{tab}{tab}return {_stringTransform.PluralizeToLower(ClassName)};";
        }

        protected virtual string GetDapperDynaParams()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}var p = new DynamicParameters();");
            output.Append(Environment.NewLine);

            output.Append($@"{tab}{tab}{tab}p.Add(""@listOf"",{GetPKMemberNamesStringList()});");

            return output.ToString();
        }

    }
}
