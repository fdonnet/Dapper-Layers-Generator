using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoGetByPkList : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoGetByPkList : GeneratorRepoGetByPkList, IMySqlGeneratorRepoGetByPkList
    {
        public MySqlGeneratorRepoGetByPkList(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
    {
        ColAndTableIdentifier = "`";
        IsBase = false;
    }

        public override string Generate()
        {
            if (PkColumns.Count() == 1)
                return base.Generate();

            if (TableSettings.GetByPkBulkGenerator)
            {
                if (!PkColumns.Any())
                    throw new ArgumentException($"You cannot run the Delete by PkList Generator for table {Table.Name}, no pk defined");

                return
                    $$"""
                    {{WriteMethodDef()}}
                    {{WriteDapperDynaParamsForPkList()}}
                    {{WriteReturnObj()}}
                    {{tab}}{{tab}}}

                    """;
            }
            return string.Empty;
        }

        protected override string WriteSqlPkListWhereClause()
        {
            return PkColumns.Count() > 1 ? string.Empty : base.WriteSqlPkListWhereClause();
        }

        protected override string WriteDapperDynaParamsForPkList()
        {
            return PkColumns.Count() > 1 ? @$"{tab}{tab}{{" : base.WriteDapperDynaParamsForPkList();
        }

        protected override string WriteReturnObj()
        {
            return PkColumns.Count() > 1
                ? $"{tab}{tab}{tab}return await GetBy{GetPkMemberNamesString()}BulkAsync({GetPKMemberNamesStringList()});"
                : base.WriteReturnObj();
        }

    }
}
