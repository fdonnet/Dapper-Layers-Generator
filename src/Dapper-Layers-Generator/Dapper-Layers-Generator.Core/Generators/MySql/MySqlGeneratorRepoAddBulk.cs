using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Microsoft.Extensions.Primitives;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoAddBulk : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoAddBulk : GeneratorRepoAddBulk, IMySqlGeneratorRepoAddBulk
    {
        public MySqlGeneratorRepoAddBulk(SettingsGlobal settingsGlobal
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
            if (TableSettings.AddBulkGenerator)
            {
                var output = new StringBuilder();
                output.Append(WriteMethodDef());
                output.Append(Environment.NewLine);
                output.Append(GetOpenTransAndInitBulkMySql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(@GetCreateDataTable());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(WriteDapperCall());
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

            if (ColumnForInsertOperations == null || !ColumnForInsertOperations.Any())
                throw new ArgumentException($"No column defined for insert (bulk), for table {Table.Name}");

            var columnsForBulk = ColumnForInsertOperations.Where(c => !c.IsAutoIncrement);

            var rowsAdd = new List<string>();
            foreach (var colBulk in columnsForBulk)
            {
                output.Append($@"{tab}{tab}{tab}table.Columns.Add(""{colBulk.Name}"",typeof({DataConverter.GetDotNetDataType(colBulk.DataType)}));" + Environment.NewLine);
                if (colBulk.IsNullable)
                {
                    output.Append($@"{tab}{tab}{tab}table.Columns[""{colBulk.Name}""]!.AllowDBNull = true;" + Environment.NewLine);
                    rowsAdd.Add($"{tab}{tab}{tab}{tab}r[\"{colBulk.Name}\"] = {ClassName.ToLower()}.{_stringTransform.PascalCase(colBulk.Name)} == null " +
                        $"? DBNull.Value : {ClassName.ToLower()}.{_stringTransform.PascalCase(colBulk.Name)};");

                }
                else
                {
                    rowsAdd.Add($@"{tab}{tab}{tab}{tab}r[""{colBulk.Name}""] = {ClassName.ToLower()}.{_stringTransform.PascalCase(colBulk.Name)};");
                }
            }

            output.Append(Environment.NewLine);
            output.Append($@"{tab}{tab}{tab}bulkCopy.DestinationTableName = ""{Table.Name}"";");
            output.Append(Environment.NewLine);
            output.Append($@"{tab}{tab}{tab}bulkCopy.BulkCopyTimeout = 600;");
            output.Append(Environment.NewLine);
            output.Append(Environment.NewLine);

            output.Append(@$"{tab}{tab}{tab}foreach(var {ClassName.ToLower()} in {_stringTransform.PluralizeToLower(ClassName)})
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

        protected override string WriteDapperCall()
        {
            return $"{tab}{tab}{tab}await bulkCopy.WriteToServerAsync(table);";
        }

    }
}
