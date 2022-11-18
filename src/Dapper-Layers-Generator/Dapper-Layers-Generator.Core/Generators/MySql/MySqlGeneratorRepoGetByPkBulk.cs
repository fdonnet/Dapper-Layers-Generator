using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (TableSettings.DeleteBulkGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                var output = new StringBuilder();
                output.Append(GetMethodDef());
                output.Append(Environment.NewLine);
                output.Append(GetOpenTransAndInitBulkMySql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetCreateDbTmpTableForPksMySql("get"));
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetCreateDataTableForPkMySql("get"));
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetBulkCallMySql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetSelectFromTmpTable());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(@GetDapperCall());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetCloseTransaction());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetReturnObj());
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}}}");
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);

                return output.ToString();
            }

            return string.Empty;
        }

        protected override string GetDapperCall()
        {
            var output = new StringBuilder($"{tab}{tab}{tab}var {_stringTransform.PluralizeToLower(ClassName)} = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QueryAsync<{ClassName}>(sql,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);");

            output.Append(Environment.NewLine);
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}var sqlDrop = \"DROP TABLE {ColAndTableIdentifier}tmp_bulkget_{Table.Name}{ColAndTableIdentifier};\";");
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sqlDrop,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);");

            return output.ToString();
        }

        protected virtual string GetSelectFromTmpTable()
        {
            var output = new StringBuilder();
            output.Append(@GetBaseSqlForSelect("t1."));
            output.Append(Environment.NewLine);
            output.Append($"{tab}{tab}{tab}INNER JOIN {ColAndTableIdentifier}tmp_bulkget_{Table.Name}{ColAndTableIdentifier} t2 ON " + Environment.NewLine +
                $"{tab}{tab}{tab}{tab}");

            //Delete fields
            var fields = String.Join($" AND ", PkColumns!.Select(c =>
                $"t1.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier} = t2.{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));

            output.Append(fields);
            output.Append("\";");

            return output.ToString();

        }

        protected override string GetReturnObj()
        {
            return $"{tab}{tab}{tab}return {_stringTransform.PluralizeToLower(ClassName)};";
        }

    }
}
