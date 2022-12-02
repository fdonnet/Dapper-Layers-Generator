using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

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

            return
                $$"""
                {{tab}}{{tab}}{{tab}}var sql = @"
                {{tab}}{{tab}}{{tab}}SELECT {{@GetColumnListStringForSelect(tableIdentifier)}}
                {{tab}}{{tab}}{{tab}}FROM {{ColAndTableIdentifier + Table.Name + ColAndTableIdentifier}} {{tableIdentifier.Replace(".", "")}}
                """;
        }

        //Can maybe be used for BULK
        protected string WriteBaseSqlForInsert()
        {
            if (ColumnForInsertOperations == null || !ColumnForInsertOperations.Any())
                throw new ArgumentException($"No column available for insert for this table{Table.Name}, genererator crash");

            return
                $$""""
                {{tab}}{{tab}}{{tab}}var sql = 
                {{tab}}{{tab}}{{tab}}"""
                {{tab}}{{tab}}{{tab}}INSERT INTO {{ColAndTableIdentifier}}{{Table.Name}}{{ColAndTableIdentifier}}
                {{tab}}{{tab}}{{tab}}(
                {{tab}}{{tab}}{{tab}}{{tab}}{{@GetColumnListStringForInsert()}}
                {{tab}}{{tab}}{{tab}})
                {{tab}}{{tab}}{{tab}}VALUES
                {{tab}}{{tab}}{{tab}}(
                """";
        }

        protected virtual string GetBaseSqlForDelete()
        {
            return
                $$"""
                {{tab}}{{tab}}{{tab}}var sql = @"
                {{tab}}{{tab}}{{tab}}DELETE FROM {{ColAndTableIdentifier}}{{Table.Name}}{{ColAndTableIdentifier}}
                """;
        }

        protected virtual string WriteValuesToInsert()
        {
            var cols = ColumnForInsertOperations!.Where(c => !c.IsAutoIncrement);

            var values = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},", cols.OrderBy(c => c.Position).Select(col =>
            {
                return $"""@{col.Name}""";
            }));

            return
                $""""
                {tab}{tab}{tab}{tab}{values}
                {tab}{tab}{tab})
                {tab}{tab}{tab}""";
                """";
        }

        protected string GetBaseSqlForUpdate()
        {
            if (ColumnForUpdateOperations == null || !ColumnForUpdateOperations.Any())
                throw new ArgumentException($"No column available for update for this table{Table.Name}, genererator crash");

            return
                $"""
                {tab}{tab}{tab}var sql = @"
                {tab}{tab}{tab}UPDATE {ColAndTableIdentifier}{Table.Name}{ColAndTableIdentifier}
                {tab}{tab}{tab}SET {GetColumnListStringForUpdate()}
                """;
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
            var spParams = String.Join(Environment.NewLine, PkColumns.Select(col =>
            {
                return $@"{tab}{tab}{tab}p.Add(""@{col.Name}"",{_stringTransform.ApplyConfigTransformMember(col.Name)});";
            }));

            return
                $"""
                {tab}{tab}{tab}var p = new DynamicParameters();
                {spParams}
                """;
        }

        protected virtual string GetDapperDynaParamsForPkList()
        {
            return
                $"""
                {tab}{tab}{tab}var p = new DynamicParameters();
                {tab}{tab}{tab}p.Add("@listOf",{GetPKMemberNamesStringList()});
                """;
        }

        private string GetColumnListStringForSelect(string tableIdentifier = "")
        {
            return String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},",
                ColumnForGetOperations!.OrderBy(c => c.Position).Select(c => $"{tableIdentifier}{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));
        }

        private string GetColumnListStringForInsert()
        {
            var cols = ColumnForInsertOperations!.Where(c => !c.IsAutoIncrement);

            return String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},",
                cols!.OrderBy(c => c.Position).Select(c => $"{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));
        }

        protected virtual string GetSqlWhereClauseForPk()
        {
            var whereClause = String.Join(Environment.NewLine + $"{tab}{tab}{tab}AND ", PkColumns.Select(col =>
            {
                return $"{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier} = @{col.Name}";
            }));
            
            return
                $"""
                {tab}{tab}{tab}WHERE {whereClause}";
                """;
        }

        protected virtual string GetSqlPkListWhereClause()
        {
            return
                $"""
                {tab}{tab}{tab}WHERE {ColAndTableIdentifier}{PkColumns.First().Name}{ColAndTableIdentifier} IN @listOf";
                """;
        }

        protected virtual string WriteOpenTransAndInitBulkMySql()
        {
            return
                $"""
                {tab}{tab}{tab}var isTransAlreadyOpen = _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction != null;
                
                {tab}{tab}{tab}if (!isTransAlreadyOpen)
                {tab}{tab}{tab}{tab}await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.OpenTransactionAsync();
                
                {tab}{tab}{tab}var bulkCopy = new MySqlBulkCopy((MySqlConnection)_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection
                {tab}{tab}{tab}{tab}, (MySqlTransaction?)_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);
                """;
        }

        protected virtual string WriteCloseTransaction()
        {
            return
                $$"""
                {{tab}}{{tab}}{{tab}}if (!isTransAlreadyOpen)
                {{tab}}{{tab}}{{tab}}{
                {{tab}}{{tab}}{{tab}}{{tab}}_dbContext.CommitTransaction();
                {{tab}}{{tab}}{{tab}}{{tab}}_dbContext.Connection.Close();
                {{tab}}{{tab}}{{tab}}}
                """;
        }

        protected virtual string GetCreateDataTableForPkMySql(string opCode)
        {
            var output = new StringBuilder();
            
            var rowsAdd = new List<string>();
            var colAdd = new List<string>();

            var indexItem = 1;
            foreach (var colBulk in PkColumns)
            {
                colAdd.Add($@"{tab}{tab}{tab}table.Columns.Add(""{colBulk.Name}"",typeof({DataConverter.GetDotNetDataType(colBulk.DataType)}));");
                rowsAdd.Add($@"{tab}{tab}{tab}{tab}r[""{colBulk.Name}""] = identity{(PkColumns.Count() > 1 ? ".Item" + indexItem : string.Empty)};");
                indexItem++;
            }

            return
                $$"""
                {{tab}}{{tab}}{{tab}}var table = new DataTable();
                {{String.Join(Environment.NewLine, colAdd)}}
                {{tab}}{{tab}}{{tab}}bulkCopy.DestinationTableName = "tmp_bulk{{opCode}}_{{Table.Name}}";
                {{tab}}{{tab}}{{tab}}bulkCopy.BulkCopyTimeout = 600;
                
                {{tab}}{{tab}}{{tab}}foreach(var identity in listOf{{string.Join("And", PkColumns.Select(c => _stringTransform.ApplyConfigTransformClass(c.Name)))}})
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
        protected virtual string GetBulkCallMySql()
        {
            return $"{tab}{tab}{tab}await bulkCopy.WriteToServerAsync(table);";
        }

        protected virtual string GetCreateDbTmpTableForPksMySql(string opCode)
        {
            //build pk columns
            var createColumns = String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab}, ", PkColumns.Select(c => c.Name + " " + c.CompleteType));

            var functionCall = $"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}" +
                $".Connection.ExecuteAsync(sqltmp,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";

            return
                $"""
                {tab}{tab}{tab}var sqltmp = @"CREATE TEMPORARY TABLE {ColAndTableIdentifier}tmp_bulk{opCode}_{Table.Name}{ColAndTableIdentifier} ({createColumns});";
                {functionCall};
                """;
        }

        protected abstract string WriteMethodDef();
        protected abstract string WriteDapperCall();
        protected abstract string WriteReturnObj();
    }
}
