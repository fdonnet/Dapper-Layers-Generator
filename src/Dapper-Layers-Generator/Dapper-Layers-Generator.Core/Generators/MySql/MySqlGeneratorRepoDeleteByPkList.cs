using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                output.Append(GetMethodDef());
                output.Append(Environment.NewLine);
                output.Append(GetDapperDynaParamsForPkList());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetReturnObj());
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}}}");
                output.Append(Environment.NewLine);
                return output.ToString();
            }

            return string.Empty;
        }

        protected override string GetSqlPkListWhereClause()
        {
            return PkColumns.Count() > 1 ? string.Empty : base.GetSqlPkListWhereClause();
        }

        protected override string GetDapperDynaParamsForPkList()
        {
            return PkColumns.Count() > 1 ? @$"{tab}{tab}{{/*Call bulk for composite pk*/" : base.GetDapperDynaParamsForPkList();
        }

        protected override string GetBaseSqlForDelete()
        {
            return PkColumns.Count() > 1 ? string.Empty : base.GetBaseSqlForDelete();
        }

        protected override string GetDapperCall()
        {
            return PkColumns.Count() > 1 ? string.Empty : base.GetDapperCall();
        }

        protected override string GetReturnObj()
        {
            return PkColumns.Count() > 1
                ? $"{tab}{tab}{tab}await DeleteBulkAsync({GetPKMemberNamesStringList()});"
                : base.GetReturnObj();
        }

    }
}
