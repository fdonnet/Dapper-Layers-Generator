using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoGetAll : IGeneratorFromTable
    {

    }
    public class GeneratorRepoGetAll : GeneratorFromTable, IGeneratorRepoGetAll
    {
        protected virtual string ColAndTableIdentifier { get; init; } = String.Empty;
        protected virtual bool IsBase { get; init; } = true;
        protected IEnumerable<IColumn>? ColumnForGetAll;

        public GeneratorRepoGetAll(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override void SetTable(string tableName)
        {
            base.SetTable(tableName);
            
            ColumnForGetAll = Table.Columns != null
                ? Table.Columns.Where(c => !TableSettings.IgnoredColumnNames.Split(',').Contains(c.Name) && !TableSettings.IgnoredColumnNamesForGet.Split(',').Contains(c.Name))
                : throw new ArgumentException($"No column available for this table{Table.Name}, genererator crash");

        }

        public override string Generate()
        {
            if (TableSettings.GetAllGenerator)
            {
                var tab = _stringTransform.IndentString;
                var output = new StringBuilder();
                output.Append(GetMethodDef());
                output.Append(Environment.NewLine);
                output.Append(@GetBaseSql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}{tab}var {_stringTransform.PluralizeToLower(ClassName)} = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QueryAsync<{ClassName}>(sql,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);");

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

        protected string GetMethodDef()
        {
            var tab = _stringTransform.IndentString;
            return $"{tab}{tab}public {(IsBase?"virtual":"override")} async Task<IEnumerable<{ClassName}>> GetAllAsync()" +
                @$"
{tab}{tab}{{";
        }

        protected string GetBaseSql()
        {
            var tab = _stringTransform.IndentString;
            var output = new StringBuilder();

            output.Append(@$"{tab}{tab}{tab}var sql = @""
{tab}{tab}{tab}SELECT {@GetColumnListString()}");
            output.Append(Environment.NewLine);
            output.Append(@$"{tab}{tab}{tab}FROM {ColAndTableIdentifier + Table.Name+ ColAndTableIdentifier}"";");

            return output.ToString();

        }

        protected string GetColumnListString()
        {
            var tab = _stringTransform.IndentString;
            var output = string.Empty;
            return String.Join(Environment.NewLine + $"{tab}{tab}{tab}{tab},",
                ColumnForGetAll!.OrderBy(c => c.Position).Select(c => $"{ColAndTableIdentifier}{c.Name}{ColAndTableIdentifier}"));
        }


    }
}
