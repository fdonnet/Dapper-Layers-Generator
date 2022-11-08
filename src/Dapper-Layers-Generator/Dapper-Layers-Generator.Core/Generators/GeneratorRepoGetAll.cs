﻿using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoGetAll : IGeneratorFromTable
    {

    }
    public class GeneratorRepoGetAll : GeneratorForOperations, IGeneratorRepoGetAll
    {
        public GeneratorRepoGetAll(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.GetAllGenerator)
            {
                var output = new StringBuilder();
                output.Append(GetMethodDef());
                output.Append(Environment.NewLine);
                output.Append(@GetBaseSql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetDapperCall());

                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}{tab}return {_stringTransform.PluralizeToLower(ClassName)};");
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}}}");
                output.Append(Environment.NewLine);

                return output.ToString();
            }

            return string.Empty;
        }

        protected override string GetMethodDef()
        {
            return $"{tab}{tab}public {(IsBase?"virtual":"override")} async Task<IEnumerable<{ClassName}>> GetAllAsync()" +
                @$"
{tab}{tab}{{";
        }

        protected string GetBaseSql()
        {
            var output = new StringBuilder();

            output.Append(@$"{tab}{tab}{tab}var sql = @""
{tab}{tab}{tab}SELECT {@GetColumnListString()}");
            output.Append(Environment.NewLine);
            output.Append(@$"{tab}{tab}{tab}FROM {ColAndTableIdentifier + Table.Name+ ColAndTableIdentifier}"";");

            return output.ToString();

        }

        protected string GetColumnListString()
        {
            var output = string.Empty;
            return String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},",
                ColumnForGetOperations!.OrderBy(c => c.Position).Select(c => $"{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));
        }

        protected override string GetDapperCall()
        {
            return  $"{tab}{tab}{tab}var {_stringTransform.PluralizeToLower(ClassName)} = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QueryAsync<{ClassName}>(sql,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

    }
}
