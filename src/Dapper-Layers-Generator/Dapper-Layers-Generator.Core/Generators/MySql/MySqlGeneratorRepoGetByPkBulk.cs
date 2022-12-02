using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorRepoGetByPkBulk : IGeneratorFromTable
    {

    }
    public class MySqlGeneratorRepoGetByPkBulk : GeneratorRepoGetByPkBulk, IMySqlGeneratorRepoGetByPkBulk
    {
        public MySqlGeneratorRepoGetByPkBulk(SettingsGlobal settingsGlobal
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
            if (TableSettings.GetByPkBulkGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                return
                    $$"""
                    {{WriteMethodDef()}}
                    {{WriteOpenTransAndInitBulkMySql()}}

                    {{WriteCreateDbTmpTableForPksMySql("get")}}

                    {{WriteCreateDataTableForPkMySql("get")}}

                    {{WriteBulkCallMySql()}}

                    {{GetSelectFromTmpTable()}}

                    {{WriteDapperCall()}}

                    {{WriteCloseTransaction()}}

                    {{WriteReturnObj()}}
                    {{tab}}{{tab}}}

                    """;
            }
            return string.Empty;
        }

        protected override string WriteDapperCall()
        {
            return
                $$"""
                {{tab}}{{tab}}{{tab}}var {{_stringTransform.PluralizeToLower(ClassName)}} = await _{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Connection.QueryAsync<{{ClassName}}>(sql,transaction:_{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Transaction);
                {{tab}}{{tab}}{{tab}}var sqlDrop = "DROP TABLE {{ColAndTableIdentifier}}tmp_bulkget_{{Table.Name}}{{ColAndTableIdentifier}};";
                {{tab}}{{tab}}{{tab}}_ = await _{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Connection.ExecuteAsync(sqlDrop,transaction:_{{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}}.Transaction);
                """;
        }

        protected virtual string GetSelectFromTmpTable()
        {
            var fields = String.Join($" AND ", PkColumns!.Select(c =>
                $"t1.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier} = t2.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));

            return
                $$""""
                {{WriteBaseSqlForSelect("t1.")}}
                {{tab}}{{tab}}{{tab}}INNER JOIN {{ColAndTableIdentifier}}tmp_bulkget_{{Table.Name}}{{ColAndTableIdentifier}} t2 ON 
                {{tab}}{{tab}}{{tab}}{{tab}}{{fields}}"
                {{tab}}{{tab}}{{tab}}""";
                """";
        }

        protected override string WriteReturnObj()
        {
            return $"{tab}{tab}{tab}return {_stringTransform.PluralizeToLower(ClassName)};";
        }

    }
}
