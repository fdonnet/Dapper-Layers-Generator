using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoAdd : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoAdd : GeneratorRepoAdd, IMySqlGeneratorRepoAdd
    {
        public MySqlGeneratorRepoAdd(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
    {
        ColAndTableIdentifier = "`";
        IsBase = false;
    }

        protected override string GetValuesToInsert()
        {
            var output = new StringBuilder();

            var cols = ColumnForInsertOperations!.Where(c => !c.IsAutoIncrement);

            var values = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},", cols.OrderBy(c => c.Position).Select(col =>
            {
                return $@"@{col.Name}";
            }));


            output.Append(values);
            output.Append(Environment.NewLine);

            if(PkColumns.Count() == 1 && PkColumns.Where(c=>c.IsAutoIncrement).Any())
            {
                //Add return identity clause
                output.Append($@"{tab}{tab}{tab});");
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}{tab}SELECT LAST_INSERT_ID();");
                output.Append($@""";");
            }
            else
            {
                output.Append($@"{tab}{tab}{tab})"";");
            }
            
            return output.ToString();
        }
    }
}
