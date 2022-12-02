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
                    {{WriteDapperDynaParamsForInsert()}}

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
