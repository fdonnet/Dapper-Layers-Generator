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
        public GeneratorForOperations(SettingsGlobal settingsGlobal
           , IReaderDBDefinitionService data
           , StringTransformationService stringTransformationService
           , IDataTypeConverter dataConverter)
               : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        protected string GetBaseSqlForSelect()
        {
            if (ColumnForGetOperations == null || !ColumnForGetOperations.Any())
                throw new ArgumentException($"No column available for select for this table{Table.Name}, genererator crash");

            var output = new StringBuilder();

            output.Append(@$"{tab}{tab}{tab}var sql = @""
{tab}{tab}{tab}SELECT {@GetColumnListStringForSelect()}");
            output.Append(Environment.NewLine);
            output.Append(@$"{tab}{tab}{tab}FROM {ColAndTableIdentifier + Table.Name + ColAndTableIdentifier}");

            return output.ToString();

        }

        //Can maybe be used for BULK
        protected string GetBaseSqlForInsert()
        {
            if (ColumnForInsertOperations == null || !ColumnForInsertOperations.Any())
                throw new ArgumentException($"No column available for insert for this table{Table.Name}, genererator crash");

            var output = new StringBuilder();

            output.Append(@$"{tab}{tab}{tab}var sql = @""
{tab}{tab}{tab}INSERT INTO {ColAndTableIdentifier}{Table.Name}{ColAndTableIdentifier}
{tab}{tab}{tab}(
");
            output.Append($"{tab}{tab}{tab}{tab}" + @GetColumnListStringForInsert());
            output.Append($@"
{tab}{tab}{tab})");
            output.Append(Environment.NewLine);
            output.Append(@$"{tab}{tab}{tab}VALUES
{tab}{tab}{tab}(
");

            return output.ToString();

        }
        protected virtual string GetValuesToInsert()
        {
            var output = new StringBuilder();

            var cols = ColumnForInsertOperations!.Where(c => !c.IsAutoIncrement);

            var values = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},", cols.OrderBy(c => c.Position).Select(col =>
            {
                return $@"@{col.Name}";
            }));


            output.Append(values);
            output.Append(Environment.NewLine);
            output.Append($@"{tab}{tab}{tab})"";");
            return output.ToString();
        }

        protected string GetBaseSqlForUpdate()
        {
            if (ColumnForUpdateOperations == null || !ColumnForUpdateOperations.Any())
                throw new ArgumentException($"No column available for update for this table{Table.Name}, genererator crash");

            var output = new StringBuilder();

            output.Append(@$"{tab}{tab}{tab}var sql = @""
{tab}{tab}{tab}UPDATE {ColAndTableIdentifier}{Table.Name}{ColAndTableIdentifier}
{tab}{tab}{tab}SET ");
            output.Append(GetColumnListStringForUpdate());
            output.Append(Environment.NewLine);

            return output.ToString();
        }

        private string GetColumnListStringForUpdate()
        {
            var output = string.Empty;
            var cols = ColumnForUpdateOperations!.Where(c => !c.IsAutoIncrement && !c.IsPrimary);

            return String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},",
                cols!.OrderBy(c => c.Position).Select(c => $"{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier} = @{c.Name}"));
        }

        protected virtual string GetDapperDynaParamsForPk()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}var p = new DynamicParameters();");
            output.Append(Environment.NewLine);

            var spParams = String.Join(Environment.NewLine, PkColumns.Select(col =>
            {
                return $@"{tab}{tab}{tab}p.Add(""@{col.Name}"",{_stringTransform.ApplyConfigTransformMember(col.Name)});";
            }));

            output.Append(spParams);
            return output.ToString();
        }


        private string GetColumnListStringForSelect()
        {
            var output = string.Empty;
            return String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},",
                ColumnForGetOperations!.OrderBy(c => c.Position).Select(c => $"{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));
        }

        private string GetColumnListStringForInsert()
        {
            var output = string.Empty;
            var cols = ColumnForInsertOperations!.Where(c => !c.IsAutoIncrement);

            return String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},",
                cols!.OrderBy(c => c.Position).Select(c => $"{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));
        }

        protected virtual string GetSqlWhereClauseForPk()
        {
            var output = new StringBuilder();

            output.Append($"{tab}{tab}{tab}WHERE ");

            var whereClause = String.Join(Environment.NewLine + $"{tab}{tab}{tab}AND ", PkColumns.Select(col =>
            {
                return $"{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier} = @{col.Name}";
            }));

            output.Append(whereClause + "\";");
            return output.ToString();

        }


        protected abstract string GetMethodDef();
        protected abstract string GetDapperCall();
        protected abstract string GetReturnObj();
    }
}
