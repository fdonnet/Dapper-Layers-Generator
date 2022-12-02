using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoDeleteByPkList : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoDeleteByPkList : GeneratorRepoDeleteByPkList, IMySqlGeneratorRepoDeleteByPkList
    {
        public MySqlGeneratorRepoDeleteByPkList(SettingsGlobal settingsGlobal
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

            if (TableSettings.DeleteByPkListGenerator)
            {
                if (!PkColumns.Any())
                    throw new ArgumentException($"You cannot run the Delete by PkList Generator for table {Table.Name}, no pk defined");

                var output = new StringBuilder();
                output.Append(WriteMethodDef());
                output.Append(Environment.NewLine);
                output.Append(WriteDapperDynaParamsForPkList());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(WriteReturnObj());
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}}}");
                output.Append(Environment.NewLine);
                return output.ToString();
            }

            return string.Empty;
        }

        protected override string WriteSqlPkListWhereClause()
        {
            return PkColumns.Count() > 1 ? string.Empty : base.WriteSqlPkListWhereClause();
        }

        protected override string WriteDapperDynaParamsForPkList()
        {
            return PkColumns.Count() > 1 ? @$"{tab}{tab}{{/*Call bulk for composite pk*/" : base.WriteDapperDynaParamsForPkList();
        }

        protected override string WriteBaseSqlForDelete()
        {
            return PkColumns.Count() > 1 ? string.Empty : base.WriteBaseSqlForDelete();
        }

        protected override string WriteDapperCall()
        {
            return PkColumns.Count() > 1 ? string.Empty : base.WriteDapperCall();
        }

        protected override string WriteReturnObj()
        {
            return PkColumns.Count() > 1
                ? $"{tab}{tab}{tab}await DeleteBulkAsync({GetPKMemberNamesStringList()});"
                : base.WriteReturnObj();
        }

    }
}
