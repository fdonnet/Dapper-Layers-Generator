using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{

    public interface IMySqlGeneratorRepoUpdateBulk : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoUpdateBulk : GeneratorRepoUpdateBulk, IMySqlGeneratorRepoUpdateBulk
    {
        public MySqlGeneratorRepoUpdateBulk(SettingsGlobal settingsGlobal
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
            return TableSettings.UpdateBulkGenerator && ColumnForUpdateOperations!.Where(c => !c.IsAutoIncrement && !c.IsPrimary).Any()
                ? $$"""
                    {{WriteMethodDef()}}
                    {{WriteOpenTransAndInitBulkMySql()}}

                    {{WriteCreateDbTmpTable()}}

                    {{WriteCreateDataTable()}}

                    {{WriteBulkCallMySql()}}

                    {{WriteUpdateFromTmpTable()}}

                    {{WriteDapperCall()}}

                    {{WriteCloseTransaction()}}
                    {{tab}}{{tab}}}

                    """
                : string.Empty;
        }

        protected virtual string WriteCreateDataTable()
        {
            if (ColumnForUpdateOperations == null || !ColumnForUpdateOperations.Any())
                throw new ArgumentException($"No column defined for update (bulk), for table {Table.Name}");

            var columnsForBulk = ColumnForUpdateOperations;

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
                {{String.Join(Environment.NewLine, colAdd)}}

                {{tab}}{{tab}}{{tab}}bulkCopy.DestinationTableName = "tmp_bulkupd_{{Table.Name}}";
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

        protected virtual string WriteCreateDbTmpTable()
        {
            return
                $$""""
                {{tab}}{{tab}}{{tab}}{{tab}}var sqltmp = $"CREATE TEMPORARY TABLE {{ColAndTableIdentifier}}tmp_bulkupd_{{Table.Name}}{{ColAndTableIdentifier}} LIKE {{ColAndTableIdentifier}}{{Table.Name}}{{ColAndTableIdentifier}};";

                {{tab}}{{tab}}{{tab}}_ = await _{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Connection.ExecuteAsync(sqltmp,transaction:_{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Transaction);
                """";
        }

        protected override string WriteDapperCall()
        {
            return
                $$"""
                {{tab}}{{tab}}{{tab}}_ = await _{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Connection.ExecuteAsync(sql,transaction:_{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Transaction);

                {{tab}}{{tab}}{{tab}}var sqlDrop = "DROP TABLE {{ColAndTableIdentifier}}tmp_bulkupd_{{Table.Name}}{{ColAndTableIdentifier}};";
                {{tab}}{{tab}}{{tab}}_ = await _{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Connection.ExecuteAsync(sqlDrop,transaction:_{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Transaction);
                """;
        }

        protected virtual string WriteUpdateFromTmpTable()
        {
            //Update fields
            var fields = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab}{tab},", ColumnForUpdateOperations!.Where(c=>!c.IsAutoIncrement).Select(c => 
                $"t1.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier} = t2.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));

            var whereClause = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab}AND ", PkColumns.Select(col =>
            {
                return $"t1.{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier} = t2.{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier}";
            }));

            return
                $$""""
                {{tab}}{{tab}}{{tab}}var sql = 
                {{tab}}{{tab}}{{tab}}"""
                {{tab}}{{tab}}{{tab}}UPDATE {{ColAndTableIdentifier}}{{Table.Name}}{{ColAndTableIdentifier}} t1, {{ColAndTableIdentifier}}tmp_bulkupd_{{Table.Name}}{{ColAndTableIdentifier}} t2
                {{tab}}{{tab}}{{tab}}{{tab}}SET
                {{tab}}{{tab}}{{tab}}{{tab}}{{fields}}
                {{tab}}{{tab}}{{tab}}{{tab}}WHERE {{whereClause}};
                {{tab}}{{tab}}{{tab}}""";
                """";
        }
    }
}
