using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoGetByUk : IGeneratorFromTable
    {

    }
    public class GeneratorRepoGetByUk : GeneratorForOperations, IGeneratorRepoGetByUk
    {
        protected KeyValuePair<string, List<Column>> _currentIndex;

        public GeneratorRepoGetByUk(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.GetByUkGenerator && ColumnNamesByIndexNameDic.Any())
            {
                var output = new StringBuilder();

                foreach (var index in ColumnNamesByIndexNameDic)
                {
                    _currentIndex = index;
                    output.Append(WriteMethodDef());
                    output.Append(Environment.NewLine);
                    output.Append(GetDapperDynaParams());
                    output.Append(Environment.NewLine);
                    output.Append(Environment.NewLine);
                    output.Append(@GetBaseSqlForSelect());
                    output.Append(Environment.NewLine);
                    output.Append(GetSqlWhereClause());
                    output.Append(Environment.NewLine);
                    output.Append(Environment.NewLine);
                    output.Append(WriteDapperCall());
                    output.Append(Environment.NewLine);
                    output.Append(Environment.NewLine);
                    output.Append(WriteReturnObj());
                    output.Append(Environment.NewLine);
                    output.Append($"{tab}{tab}}}");
                    output.Append(Environment.NewLine);
                }

                return output.ToString();
            }

            return string.Empty;
        }

        protected override string WriteMethodDef()
        {
            return $"{tab}{tab}public {(IsBase ? "virtual" : "override")} async Task<{ClassName}?> GetBy{GetUkMemberNamesString(_currentIndex.Key)}Async({GetUkMemberNamesStringAndType(_currentIndex.Key)})" +
                @$"
{tab}{tab}{{";
        }

        protected override string WriteDapperCall()
        {
            return $"{tab}{tab}{tab}var {ClassName.ToLower()} = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QuerySingleOrDefaultAsync<{ClassName}>(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected virtual string GetSqlWhereClause()
        {
            var output = new StringBuilder();

            output.Append($"{tab}{tab}{tab}WHERE ");

            var whereClause = String.Join(Environment.NewLine + $"{tab}{tab}{tab}AND ", _currentIndex.Value.Select(col =>
            {
                return $"{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier} = @{col.Name}";
            }));

            output.Append(whereClause + "\";");
            return output.ToString();

        }

        protected virtual string GetDapperDynaParams()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}var p = new DynamicParameters();");
            output.Append(Environment.NewLine);

            var spParams = String.Join(Environment.NewLine, _currentIndex.Value.Select(col =>
            {
                return $@"{tab}{tab}{tab}p.Add(""@{col.Name}"",{_stringTransform.ApplyConfigTransformMember(col.Name)});";
            }));

            output.Append(spParams);
            return output.ToString();
        }

        protected override string WriteReturnObj()
        {
            return $"{tab}{tab}{tab}return {ClassName.ToLower()};";
        }
    }
}
