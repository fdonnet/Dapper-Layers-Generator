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
            if (TableSettings.UpdateBulkGenerator && ColumnForUpdateOperations!.Where(c => !c.IsAutoIncrement && !c.IsPrimary).Any())
            {
                var output = new StringBuilder();
                output.Append(WriteMethodDef());
                output.Append(Environment.NewLine);
                output.Append(WriteOpenTransAndInitBulkMySql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetCreateDbTmpTable());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetCreateDataTable());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetBulkCallMySql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(@GetUpdateFromTmpTable());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(WriteDapperCall());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(WriteCloseTransaction());
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

            if (ColumnForUpdateOperations == null || !ColumnForUpdateOperations.Any())
                throw new ArgumentException($"No column defined for update (bulk), for table {Table.Name}");

            var columnsForBulk = ColumnForUpdateOperations;

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
            output.Append($@"{tab}{tab}{tab}bulkCopy.DestinationTableName = ""tmp_bulkupd_{Table.Name}"";");
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

        protected virtual string GetCreateDbTmpTable()
        {
            var output = new StringBuilder();

            output.Append($"{tab}{tab}{tab}var sqltmp = \"CREATE TEMPORARY TABLE " +
                $"{ColAndTableIdentifier}tmp_bulkupd_{Table.Name}{ColAndTableIdentifier} LIKE {ColAndTableIdentifier}{Table.Name}{ColAndTableIdentifier};\";");

            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sqltmp,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);");

            return output.ToString();

        }

        protected override string WriteDapperCall()
        {
            var output = new StringBuilder($"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sql,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);");

            output.Append(Environment.NewLine);
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}var sqlDrop = \"DROP TABLE {ColAndTableIdentifier}tmp_bulkupd_{Table.Name}{ColAndTableIdentifier};\";");
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sqlDrop,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);");

            return output.ToString();
        }

        protected virtual string GetUpdateFromTmpTable()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}var sql = @\"UPDATE {ColAndTableIdentifier}{Table.Name}{ColAndTableIdentifier} t1, " +
                $"{ColAndTableIdentifier}tmp_bulkupd_{Table.Name}{ColAndTableIdentifier} t2" + Environment.NewLine +
                $"{tab}{tab}{tab}{tab}SET" + Environment.NewLine + $"{tab}{tab}{tab}{tab}");

            //Update fields
            var fields = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab}{tab},", ColumnForUpdateOperations!.Where(c=>!c.IsAutoIncrement).Select(c => 
                $"t1.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier} = t2.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));

            output.Append(fields);
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}{tab}WHERE ");

            var whereClause = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab}AND ", PkColumns.Select(col =>
            {
                return $"t1.{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier} = t2.{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier}";
            }));

            output.Append(whereClause + "\";");

            return output.ToString();

        }
    }
}
