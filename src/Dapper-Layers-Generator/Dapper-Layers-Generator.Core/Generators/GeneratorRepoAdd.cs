using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoAdd : IGeneratorFromTable
    {

    }
    public class GeneratorRepoAdd : GeneratorForOperations, IGeneratorRepoAdd
    {

        public GeneratorRepoAdd(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }
        public override string Generate()
        {
            return TableSettings.AddGenerator
                ? $$"""
                    {{WriteMethodDef()}}
                    {{WriteDapperDynaParams()}}

                    {{WriteBaseSqlForInsert()}}
                    {{WriteValuesToInsert()}}

                    {{WriteDapperCall()}}{{WriteReturnObj()}}
                    {{tab}}{{tab}}}

                    """
                : string.Empty;
        }

        protected override string WriteMethodDef()
        {
            return PkColumns.Count() == 1 && PkColumns.Where(c => c.IsAutoIncrement).Any()
                ? $$"""
                    {{tab}}{{tab}}public {{(IsBase ? "virtual" : "override")}} async Task<{{GetPkMemberTypes()}}> AddAsync({{ClassName}} {{_stringTransform.ApplyConfigTransformMember(ClassName)}})
                    {{tab}}{{tab}}{
                    """
                : $$"""
                    {{tab}}{{tab}}public {{(IsBase ? "virtual" : "override")}} async Task AddAsync({{ClassName}} {{_stringTransform.ApplyConfigTransformMember(ClassName)}})
                    {{tab}}{{tab}}{
                    """;
        }

        protected override string WriteDapperCall()
        {
            if (PkColumns.Count() == 1 && PkColumns.Where(c => c.IsAutoIncrement).Any())
                return $"{tab}{tab}{tab}var identity = " +
                        $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                        $"ExecuteScalarAsync<{GetPkMemberTypes()}>(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
            else
                return $"{tab}{tab}{tab}_ = " +
                $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                $"ExecuteAsync(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";

        }

        protected virtual string WriteDapperDynaParams()
        {
            var cols = ColumnForInsertOperations!.Where(c => !c.IsAutoIncrement);

            var spParams = String.Join(Environment.NewLine, cols.OrderBy(c => c.Position).Select(col =>
            {
                return $@"{tab}{tab}{tab}p.Add(""@{col.Name}"", {_stringTransform.ApplyConfigTransformMember(ClassName)}.{_stringTransform.PascalCase(col.Name)});";
            }));

            return
                $"""
                {tab}{tab}{tab}var p = new DynamicParameters();
                {spParams}
                """;
        }

        protected override string WriteReturnObj()
        {
            //The base implementation is very minimal (no real return from the DB, need to be override by dbprovider specific)
            if (PkColumns.Count() == 1 && PkColumns.Where(c => c.IsAutoIncrement).Any())
            {
                return 
                    $"""

                    {tab}{tab}{tab}return identity;
                    """;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
