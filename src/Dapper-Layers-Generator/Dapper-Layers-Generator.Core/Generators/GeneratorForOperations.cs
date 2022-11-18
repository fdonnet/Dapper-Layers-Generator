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

        protected string GetBaseSqlForSelect(string tableIdentifier = "")
        {
            if (ColumnForGetOperations == null || !ColumnForGetOperations.Any())
                throw new ArgumentException($"No column available for select for this table{Table.Name}, genererator crash");

            var output = new StringBuilder();

            output.Append(@$"{tab}{tab}{tab}var sql = @""
{tab}{tab}{tab}SELECT {@GetColumnListStringForSelect(tableIdentifier)}");
            output.Append(Environment.NewLine);
            output.Append(@$"{tab}{tab}{tab}FROM {ColAndTableIdentifier + Table.Name + ColAndTableIdentifier} {tableIdentifier.Replace(".","")}");

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

        protected virtual string GetBaseSqlForDelete()
        {
            var output = new StringBuilder();

            output.Append(@$"{tab}{tab}{tab}var sql = @""
{tab}{tab}{tab}DELETE FROM {ColAndTableIdentifier}{Table.Name}{ColAndTableIdentifier}");

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

        protected virtual string GetDapperDynaParamsForPkList()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}var p = new DynamicParameters();");
            output.Append(Environment.NewLine);

            output.Append($@"{tab}{tab}{tab}p.Add(""@listOf"",{GetPKMemberNamesStringList()});");

            return output.ToString();
        }


        private string GetColumnListStringForSelect(string tableIdentifier = "")
        {
            var output = string.Empty;
            return String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},",
                ColumnForGetOperations!.OrderBy(c => c.Position).Select(c => $"{tableIdentifier}{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));
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

        protected virtual string GetSqlPkListWhereClause()
        {
            var output = new StringBuilder();

            output.Append($"{tab}{tab}{tab}WHERE ");
            output.Append($"{ColAndTableIdentifier}{PkColumns.First().Name}{ColAndTableIdentifier} IN @listOf");

            output.Append("\";");
            return output.ToString();

        }

        protected virtual string GetOpenTransAndInitBulkMySql()
        {

            return $@"{tab}{tab}{tab}var isTransAlreadyOpen = _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction != null;

{tab}{tab}{tab}if (!isTransAlreadyOpen)
{tab}{tab}{tab}{tab}await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.OpenTransactionAsync();

{tab}{tab}{tab}var bulkCopy = new MySqlBulkCopy((MySqlConnection)_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection
{tab}{tab}{tab}{tab}, (MySqlTransaction?)_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";

        }

        protected virtual string GetCloseTransaction()
        {
            return $@"{tab}{tab}{tab}if (!isTransAlreadyOpen)
{tab}{tab}{tab}{{
{tab}{tab}{tab}{tab}_dbContext.CommitTransaction();
{tab}{tab}{tab}{tab}_dbContext.Connection.Close();
{tab}{tab}{tab}}}";
        }

        protected virtual string GetCreateDataTableForPkMySql(string opCode)
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}var table = new DataTable();" + Environment.NewLine);

            var rowsAdd = new List<string>();
            var indexItem = 1;
            foreach (var colBulk in PkColumns)
            {
                output.Append($@"{tab}{tab}{tab}table.Columns.Add(""{colBulk.Name}"",typeof({DataConverter.GetDotNetDataType(colBulk.DataType)}));" + Environment.NewLine);
                rowsAdd.Add($@"{tab}{tab}{tab}{tab}r[""{colBulk.Name}""] = identity{(PkColumns.Count() > 1 ? ".Item" + indexItem : string.Empty)};");
                indexItem++;
            }

            output.Append(Environment.NewLine);
            output.Append($@"{tab}{tab}{tab}bulkCopy.DestinationTableName = ""tmp_bulk{opCode}_{Table.Name}"";");
            output.Append(Environment.NewLine);
            output.Append($@"{tab}{tab}{tab}bulkCopy.BulkCopyTimeout = 600;");
            output.Append(Environment.NewLine);
            output.Append(Environment.NewLine);

            output.Append(@$"{tab}{tab}{tab}foreach(var identity in listOf{string.Join("And", PkColumns.Select(c => _stringTransform.ApplyConfigTransformClass(c.Name)))})
{tab}{tab}{tab}{{
{tab}{tab}{tab}{tab}DataRow r = table.NewRow();");

            output.Append(Environment.NewLine);
            output.Append(String.Join(Environment.NewLine, rowsAdd));
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}{tab}table.Rows.Add(r);");
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}}}");
            output.Append(Environment.NewLine);
            output.Append(Environment.NewLine);

            output.Append($@"{tab}{tab}{tab}List<MySqlBulkCopyColumnMapping> colMappings = new();
{tab}{tab}{tab}int i = 0;
{tab}{tab}{tab}foreach (DataColumn col in table.Columns)
{tab}{tab}{tab}{{
{tab}{tab}{tab}{tab}colMappings.Add(new MySqlBulkCopyColumnMapping(i, col.ColumnName));
{tab}{tab}{tab}{tab}i++;
{tab}{tab}{tab}}}

{tab}{tab}{tab}bulkCopy.ColumnMappings.AddRange(colMappings);");

            return output.ToString();
        }
        protected virtual string GetBulkCallMySql()
        {
            return $"{tab}{tab}{tab}await bulkCopy.WriteToServerAsync(table);";
        }

        protected virtual string GetCreateDbTmpTableForPksMySql(string opCode)
        {
            var output = new StringBuilder();

            output.Append($"{tab}{tab}{tab}var sqltmp = @\"CREATE TEMPORARY TABLE " +
                $"{ColAndTableIdentifier}tmp_bulk{opCode}_{Table.Name}{ColAndTableIdentifier} (");

            //build pk columns
            var createColumns = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab}, ", PkColumns.Select(c => c.Name + " " + c.CompleteType));
            output.Append(createColumns + ");\";");
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sqltmp,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);");

            return output.ToString();

        }

        protected abstract string GetMethodDef();
        protected abstract string GetDapperCall();
        protected abstract string GetReturnObj();
    }
}
