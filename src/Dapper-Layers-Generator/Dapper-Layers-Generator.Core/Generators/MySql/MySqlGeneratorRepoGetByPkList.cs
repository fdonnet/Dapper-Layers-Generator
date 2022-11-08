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

        protected override string GetSqlWhereClause()
        {
            if (PkColumns.Count() > 1)
               return string.Empty;

            return base.GetSqlWhereClause();

        }

        protected override string GetDapperDynaParams()
        {
            if (PkColumns.Count() > 1)
                return @$"{tab}{tab}{{/*Not Implemented YET for mySQL !!!""";

            return base.GetDapperDynaParams();
        }

        protected override string GetReturnObj()
        {
            if (PkColumns.Count() > 1)
                return $"-------------  */throw new NotImplementedException(\"Select by PKList with composite pk\");";

            return base.GetReturnObj();
        }

    }
}
