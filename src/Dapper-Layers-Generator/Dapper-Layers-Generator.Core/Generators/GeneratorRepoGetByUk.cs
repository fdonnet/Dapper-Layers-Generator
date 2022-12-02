using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System.Text;

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

                    var toBeAdded =
                        $$"""
                        {{WriteMethodDef()}}
                        {{WriteDapperDynaParams()}}

                        {{WriteBaseSqlForSelect()}}
                        {{WriteSqlWhereClause()}}

                        {{WriteDapperCall()}}

                        {{WriteReturnObj()}}
                        {{tab}}{{tab}}}

                        """;

                    output.Append(toBeAdded);
                }

                return output.ToString();
            }

            return string.Empty;
        }

        protected override string WriteMethodDef()
        {
            return
                $$"""
                {{tab}}{{tab}}public {{(IsBase ? "virtual" : "override")}} async Task<{{ClassName}}?> GetBy{{GetUkMemberNamesString(_currentIndex.Key)}}Async({{GetUkMemberNamesStringAndType(_currentIndex.Key)}})
                {{tab}}{{tab}}{
                """;
        }

        protected override string WriteDapperCall()
        {
            return $"{tab}{tab}{tab}var {ClassName.ToLower()} = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QuerySingleOrDefaultAsync<{ClassName}>(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected virtual string WriteSqlWhereClause()
        {
            var whereClause = String.Join(Environment.NewLine + $"{tab}{tab}{tab}AND ", _currentIndex.Value.Select(col =>
            {
                return $"{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier} = @{col.Name}";
            }));

            return
                $$""""
                {{tab}}{{tab}}{{tab}}WHERE {{whereClause}}
                {{tab}}{{tab}}{{tab}}""";
                """";
        }

        protected virtual string WriteDapperDynaParams()
        {
            var spParams = String.Join(Environment.NewLine, _currentIndex.Value.Select(col =>
            {
                return $@"{tab}{tab}{tab}p.Add(""@{col.Name}"",{_stringTransform.ApplyConfigTransformMember(col.Name)});";
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
