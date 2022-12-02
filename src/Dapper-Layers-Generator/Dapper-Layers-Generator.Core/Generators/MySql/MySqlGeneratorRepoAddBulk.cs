using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using MySqlX.XDevAPI.Relational;
using System.Text;

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
                return
                    $$"""
                    {{WriteMethodDef()}}
                    {{WriteOpenTransAndInitBulkMySql()}}

                    {{WriteCreateDataTable()}}

                    {{WriteDapperCall()}}

                    {{WriteCloseTransaction()}}
                    {{tab}}{{tab}}}

                    """;
            }

            return string.Empty;
        }

        protected virtual string WriteCreateDataTable()
        {
            var output = new StringBuilder();

            if (ColumnForInsertOperations == null || !ColumnForInsertOperations.Any())
                throw new ArgumentException($"No column defined for insert (bulk), for table {Table.Name}");

            var columnsForBulk = ColumnForInsertOperations.Where(c => !c.IsAutoIncrement);

            var rowsAdd = new List<string>();
            var colAdd = new List<string>();
            foreach (var colBulk in columnsForBulk)
            {
                colAdd.Add($@"{tab}{tab}{tab}table.Columns.Add(""{colBulk.Name}"",typeof({DataConverter.GetDotNetDataType(colBulk.DataType)}));");
                if (colBulk.IsNullable)
                {
                    colAdd.Add($@"{tab}{tab}{tab}table.Columns[""{colBulk.Name}""]!.AllowDBNull = true;");
                    rowsAdd.Add($"{tab}{tab}{tab}{tab}r[\"{colBulk.Name}\"] = {ClassName.ToLower()}.{_stringTransform.PascalCase(colBulk.Name)} == null " +
                        $"? DBNull.Value : {ClassName.ToLower()}.{_stringTransform.PascalCase(colBulk.Name)};");

                }
                else
                {
                    rowsAdd.Add($@"{tab}{tab}{tab}{tab}r[""{colBulk.Name}""] = {ClassName.ToLower()}.{_stringTransform.PascalCase(colBulk.Name)};");
                }
            }

            return
                $$"""
                {{tab}}{{tab}}{{tab}}var table = new DataTable();
                {{String.Join(Environment.NewLine,colAdd)}}

                {{tab}}{{tab}}{{tab}}bulkCopy.DestinationTableName = "{{Table.Name}}";
                {{tab}}{{tab}}{{tab}}bulkCopy.BulkCopyTimeout = 600;

                {{tab}}{{tab}}{{tab}}foreach(var {{ClassName.ToLower()}} in {{_stringTransform.PluralizeToLower(ClassName)}})
                {{tab}}{{tab}}{{tab}}{
                {{tab}}{{tab}}{{tab}}{{tab}}DataRow r = table.NewRow();
                {{String.Join(Environment.NewLine, rowsAdd)}}
                {{tab}}{{tab}}{{tab}}{{tab}}table.Rows.Add(r);
                {{tab}}{{tab}}{{tab}}}

                {{tab}}{{tab}}{{tab}}List<MySqlBulkCopyColumnMapping> colMappings = new();
                {{tab}}{{tab}}{{tab}}int i = 0;
                {{tab}}{{tab}}{{tab}}foreach (DataColumn col in table.Columns)
                {{tab}}{{tab}}{{tab}}{
                {{tab}}{{tab}}{{tab}}{{tab}}colMappings.Add(new MySqlBulkCopyColumnMapping(i, col.ColumnName));
                {{tab}}{{tab}}{{tab}}{{tab}}i++;
                {{tab}}{{tab}}{{tab}}}
                
                {{tab}}{{tab}}{{tab}}bulkCopy.ColumnMappings.AddRange(colMappings);
                """;
        }

        protected override string WriteDapperCall()
        {
            return $"{tab}{tab}{tab}await bulkCopy.WriteToServerAsync(table);";
        }

    }
}
