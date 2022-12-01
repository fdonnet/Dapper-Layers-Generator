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
                output.Append(WriteMethodDef());
                output.Append(Environment.NewLine);
                output.Append(GetOpenTransAndInitBulkMySql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetCreateDbTmpTableForPksMySql("delete"));
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetCreateDataTableForPkMySql("delete"));
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetBulkCallMySql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetDeleteFromTmpTable());
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


        protected override string WriteDapperCall()
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
            output.Append("\";");

            return output.ToString();

        }

    }
}
