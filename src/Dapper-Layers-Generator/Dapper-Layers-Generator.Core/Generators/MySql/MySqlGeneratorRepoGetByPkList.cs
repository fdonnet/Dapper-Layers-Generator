using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected override string GetSqlPkListWhereClause()
        {
            return PkColumns.Count() > 1 ? string.Empty : base.GetSqlPkListWhereClause();
        }

        protected override string GetDapperDynaParamsForPkList()
        {
            return PkColumns.Count() > 1 ? @$"{tab}{tab}{{/*Not Implemented for mySQL !!!""" : base.GetDapperDynaParamsForPkList();
        }

        protected override string GetReturnObj()
        {
            return PkColumns.Count() > 1
                ? $"-------------  */throw new NotImplementedException(\"Select by PKList with composite pk, use GetByPkListBulk funtion\");"
                : base.GetReturnObj();
        }

    }
}
