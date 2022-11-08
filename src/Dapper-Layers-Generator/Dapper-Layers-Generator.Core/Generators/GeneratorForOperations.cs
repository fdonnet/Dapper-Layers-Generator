using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public abstract class GeneratorForOperations : GeneratorFromTable
    {
        protected IEnumerable<IColumn>? ColumnForGetOperations;

        public GeneratorForOperations(SettingsGlobal settingsGlobal
           , IReaderDBDefinitionService data
           , StringTransformationService stringTransformationService
           , IDataTypeConverter dataConverter)
               : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {
            
        }

        public override void SetTable(string tableName)
        {
            base.SetTable(tableName);

            ColumnForGetOperations = Table.Columns != null
                ? Table.Columns.Where(c => !TableSettings.IgnoredColumnNames.Split(',').Contains(c.Name) && !TableSettings.IgnoredColumnNamesForGet.Split(',').Contains(c.Name))
                : throw new ArgumentException($"No column available for this table{Table.Name}, genererator crash");
        }

        protected string GetBaseSqlForSelect()
        {
            var output = new StringBuilder();

            output.Append(@$"{tab}{tab}{tab}var sql = @""
{tab}{tab}{tab}SELECT {@GetColumnListStringForSelect()}");
            output.Append(Environment.NewLine);
            output.Append(@$"{tab}{tab}{tab}FROM {ColAndTableIdentifier + Table.Name + ColAndTableIdentifier}");

            return output.ToString();

        }

        private string GetColumnListStringForSelect()
        {
            var output = string.Empty;
            return String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},",
                ColumnForGetOperations!.OrderBy(c => c.Position).Select(c => $"{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));
        }


        protected abstract string GetMethodDef();
        protected abstract string GetDapperCall();
        protected abstract string GetReturnObj();
    }
}
