using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoGetByPk : IGeneratorFromTable
    {

    }

    public class GeneratorRepoGetByPk : GeneratorForOperations, IGeneratorRepoGetByPk
    {
        public GeneratorRepoGetByPk(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.GetByPkGenerator)
            {
                if (PkColumns.Count() == 0)
                    throw new ArgumentException($"You cannot run the Get by Pk Generator for table {Table.Name}, no pk defined");

                var output = new StringBuilder();
                output.Append(GetMethodDef());
                output.Append(Environment.NewLine);
                output.Append(GetDapperDynaParams());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(@GetBaseSqlForSelect());
                output.Append(Environment.NewLine);
                output.Append(GetSqlWhereClause());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetDapperCall());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetReturnObj());
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}}}");
                output.Append(Environment.NewLine);

                return output.ToString();
            }

            return string.Empty;
        }

        protected override string GetMethodDef()
        {
            return $"{tab}{tab}public {(IsBase ? "virtual" : "override")} async Task<{ClassName}> GetBy{GetPkMemberNamesString()}Async({GetPkMemberNamesStringAndType()})" +
                @$"
{tab}{tab}{{";
        }

        protected override string GetDapperCall()
        {
            return $"{tab}{tab}{tab}var {ClassName.ToLower()} = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QuerySingleOrDefaultAsync<{ClassName}>(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected virtual string GetSqlWhereClause()
        {
            var output = new StringBuilder();

            output.Append($"{tab}{tab}{tab}WHERE ");

            var whereClause = String.Join(Environment.NewLine + $"{tab}{tab}{tab}AND ", PkColumns.Select(col =>
            {
                return $"{ColAndTableIdentifier}{col.Name}{ColAndTableIdentifier} = @{col.Name}";
            }));

            output.Append(whereClause + "\";");
            return output.ToString();

        }

        protected virtual string GetDapperDynaParams()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}var p = new DynamicParameters();");
            output.Append(Environment.NewLine);

            var spParams = String.Join(Environment.NewLine, PkColumns.Select(col =>
            {
                return $@"{tab}{tab}{tab}p.Add(""@{col.Name}"",{_stringTransform.ApplyConfigTransformMember(col.Name)});";
            }));

            output.Append(spParams);
            return output.ToString();
        }

        protected override string GetReturnObj()
        {
            return $"{tab}{tab}{tab}return {ClassName.ToLower()};";
        }

    }
}
