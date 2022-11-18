using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoDeleteBulk : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoDeleteBulk : GeneratorRepoDeleteBulk, IMySqlGeneratorRepoDeleteBulk
    {
        public MySqlGeneratorRepoDeleteBulk(SettingsGlobal settingsGlobal
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
            if (TableSettings.DeleteBulkGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                var output = new StringBuilder();
                output.Append(GetMethodDef());
                output.Append(Environment.NewLine);
                output.Append(GetOpenTransAndInitBulkMySql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetCreateDbTmpTable());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetCreateDataTable());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetBulkCall());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetDeleteFromTmpTable());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(@GetDapperCall());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetCloseTransaction());
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}}}");
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);

                return output.ToString();
            }

            return string.Empty;
        }

        protected virtual string GetCreateDataTable()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}var table = new DataTable();" + Environment.NewLine);

            var rowsAdd = new List<string>();
            var indexItem = 1;
            foreach (var colBulk in PkColumns)
            {
                output.Append($@"{tab}{tab}{tab}table.Columns.Add(""{colBulk.Name}"",typeof({DataConverter.GetDotNetDataType(colBulk.DataType)}));" + Environment.NewLine);
                rowsAdd.Add($@"{tab}{tab}{tab}{tab}r[""{colBulk.Name}""] = identity{(PkColumns.Count()>1 ? ".Item"+indexItem:string.Empty)};");
                indexItem++;
            }

            output.Append(Environment.NewLine);
            output.Append($@"{tab}{tab}{tab}bulkCopy.DestinationTableName = ""tmp_bulkdelete_{Table.Name}"";");
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
        protected virtual string GetBulkCall()
        {
            return $"{tab}{tab}{tab}await bulkCopy.WriteToServerAsync(table);";
        }

        //For the moment a complete tmp table is created but we can change that to create only a pklist tmp table
        protected virtual string GetCreateDbTmpTable()
        {
            var output = new StringBuilder();

            output.Append($"{tab}{tab}{tab}var sqltmp = @\"CREATE TEMPORARY TABLE " +
                $"{ColAndTableIdentifier}tmp_bulkdelete_{Table.Name}{ColAndTableIdentifier} (");

            //build pk columns
            var createColumns = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab}, ", PkColumns.Select(c => c.Name + " " + c.CompleteType));
            output.Append(createColumns + ");\";");
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sqltmp,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);");

            return output.ToString();

        }
        protected override string GetDapperCall()
        {
            var output = new StringBuilder($"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sql,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);");

            output.Append(Environment.NewLine);
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}var sqlDrop = \"DROP TABLE {ColAndTableIdentifier}tmp_bulkdelete_{Table.Name}{ColAndTableIdentifier};\";");
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sqlDrop,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);");

            return output.ToString();
        }

        protected virtual string GetDeleteFromTmpTable()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}var sql = @\"DELETE t1 FROM {ColAndTableIdentifier}{Table.Name}{ColAndTableIdentifier} t1" + Environment.NewLine +
                $"{tab}{tab}{tab}{tab}INNER JOIN {ColAndTableIdentifier}tmp_bulkdelete_{Table.Name}{ColAndTableIdentifier} t2 ON " + Environment.NewLine +
                $"{tab}{tab}{tab}{tab}");

            //Delete fields
            var fields = String.Join($" AND ", PkColumns!.Select(c =>
                $"t1.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier} = t2.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));

            output.Append(fields);
            //output.Append(Environment.NewLine);
            //output.Append($"{tab}{tab}{tab}{tab}WHERE ");

            //var whereClause = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab}AND ", PkColumns.Select(col =>
            //{
            //    return $"t1.{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier} = t2.{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier}";
            //}));

            output.Append("\";");

            return output.ToString();

        }

    }
}
