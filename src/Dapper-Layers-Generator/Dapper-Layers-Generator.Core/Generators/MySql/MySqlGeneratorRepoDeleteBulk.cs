using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

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
                return
                    $$"""
                    {{WriteMethodDef()}}
                    {{WriteOpenTransAndInitBulkMySql()}}

                    {{WriteCreateDbTmpTableForPksMySql("delete")}}

                    {{WriteCreateDataTableForPkMySql("delete")}}

                    {{WriteBulkCallMySql()}}

                    {{WriteDeleteFromTmpTable()}}
                    
                    {{WriteDapperCall()}}

                    {{WriteCloseTransaction()}}
                    {{tab}}{{tab}}}

                    """;
            }

            return string.Empty;
        }

        protected override string WriteDapperCall()
        {
            return
                $$"""
                {{tab}}{{tab}}{{tab}}_ = await _{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Connection.ExecuteAsync(sql,transaction:_{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Transaction);

                {{tab}}{{tab}}{{tab}}var sqlDrop = "DROP TABLE {{ColAndTableIdentifier}}tmp_bulkdelete_{{Table.Name}}{{ColAndTableIdentifier}};";

                {{tab}}{{tab}}{{tab}}_ = await _{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Connection.ExecuteAsync(sqlDrop,transaction:_{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Transaction);
                """;
        }

        protected virtual string WriteDeleteFromTmpTable()
        {
            //Delete fields
            var fields = String.Join($" AND ", PkColumns!.Select(c =>
                $"t1.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier} = t2.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));

            return
                $$""""
                {{tab}}{{tab}}{{tab}}var sql = 
                {{tab}}{{tab}}{{tab}}"""
                {{tab}}{{tab}}{{tab}}DELETE t1 FROM {{ColAndTableIdentifier}}{{Table.Name}}{{ColAndTableIdentifier}} t1
                {{tab}}{{tab}}{{tab}}INNER JOIN {{ColAndTableIdentifier}}tmp_bulkdelete_{{Table.Name}}{{ColAndTableIdentifier}} t2 ON 
                {{tab}}{{tab}}{{tab}}{{fields}};
                {{tab}}{{tab}}{{tab}}""";
                """";
        }

    }
}
