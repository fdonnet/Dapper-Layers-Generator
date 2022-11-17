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

    }
}
