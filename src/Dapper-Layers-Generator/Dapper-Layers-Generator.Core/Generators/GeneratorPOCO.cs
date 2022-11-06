using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorPOCO : IGenerator, IGeneratorFromTable
    {

    }

    public class GeneratorPOCO : GeneratorFromTable, IGeneratorPOCO
    {
        public GeneratorPOCO(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (Table == null)
                throw new NullReferenceException("Cannot use POCO generator without a loaded table (use SetTable)");


            string output = WritePocoClass();

            return output;
        }

        private string WritePocoHeaderComment()
        {
            var tab = _stringTransform.IndentString;
            return $@"#nullable disable warnings
namespace {_settings.TargetNamespaceForPOCO} 
{{
{tab}/// =================================================================
{tab}/// <summary>
{tab}/// Poco class for the table {Table.Name}
{tab}/// Author: {_settings.AuthorName}
{tab}/// Poco: {ClassName}
{tab}/// Generated: {DateTime.Now}
{tab}/// WARNING: Never change this file manually (re-generate it)
{tab}/// </summary>
{tab}/// =================================================================";

        }

        private string WritePocoClass()
        {
            var tab = _stringTransform.IndentString;
            var builder = new StringBuilder();
            
            builder.Append(@WritePocoHeaderComment());
            
            builder.Append(Environment.NewLine);
            builder.Append($"{tab}public class {ClassName}");
            builder.Append(Environment.NewLine);
            builder.Append($"{tab}{{");
            builder.Append(Environment.NewLine);

            builder.Append(WritePocoMemberFields());

            builder.Append($"{tab}}}");
            builder.Append(Environment.NewLine);
            builder.Append("}");

            return builder.ToString();
            
        }

        private string WritePocoMemberFields()
        {
            var tab = _stringTransform.IndentString;

            if (Table.Columns == null)
                return "";

            var columns = string.IsNullOrEmpty(TableSettings.IgnoredColumnNames)
                          ? Table.Columns
                          : Table.Columns.Where(c => !TableSettings.IgnoredColumnNames.Split(',').Contains(c.Name));

            var members = String.Join(Environment.NewLine, columns.Select(col =>
            {
                var memberName = _stringTransform.PascalCase(col.Name);
                var colSettings = TableSettings.GetColumnSettings(col.Name);

                var memberType = colSettings.FieldNameCustomType == String.Empty
                                    ? DataConverter.GetDotNetDataType(col.DataType, col.IsNullable)
                                    : colSettings.FieldNameCustomType;

                var decorators = WriteMemberDecorators(colSettings, memberType, col);

                return  $"{decorators}{tab}{tab}public {memberType} {memberName} {{ get; set; }}"
                    + Environment.NewLine;

            }));

            return members;
        }

        private string WriteMemberDecorators(SettingsColumn settings, string memberType, IColumn col)
        {
            var decorators = new StringBuilder();
            var curDecoratorsLength = 0;

            decorators.Append(WriteMemberRequieredDecorator(settings, col));
            SpaceBetweenDecorators(ref decorators, ref curDecoratorsLength);

            decorators.Append(WriteMemberStringDecorator(settings, memberType, col));
            SpaceBetweenDecorators(ref decorators, ref curDecoratorsLength);

            decorators.Append(WriteMemberJsonIgnoreDecorator(col));
            SpaceBetweenDecorators(ref decorators, ref curDecoratorsLength);

            decorators.Append(WriteMemberCustomDecorator(settings));
            SpaceBetweenDecorators(ref decorators, ref curDecoratorsLength);

            return decorators.ToString();
        }

        private string WriteMemberStringDecorator(SettingsColumn settings, string memberType, IColumn col)
        {
            var tab = _stringTransform.IndentString;
            var decorator = string.Empty;

            if (memberType == "string")
            {
                if (settings.StandardStringLengthDecorator)
                {
                    if (col.Length > 0)
                    {
                        decorator = $"{tab}{tab}[System.ComponentModel.DataAnnotations.StringLength({col.Length})]";
                    }
                }
            }
            return decorator;
        }

        private string WriteMemberRequieredDecorator(SettingsColumn settings, IColumn col)
        {
            var tab = _stringTransform.IndentString;
            var decorator = string.Empty;

            if (!col.IsNullable && !col.IsAutoIncrement)
            {
                if (settings.StandardRequiredDecorator)
                {
                    decorator = $"{tab}{tab}[System.ComponentModel.DataAnnotations.Required]";
                }
            }
            return decorator;
        }

        private string WriteMemberJsonIgnoreDecorator(IColumn col)
        {
            var tab = _stringTransform.IndentString;
            var decorator = string.Empty;
            var colFound = TableSettings.JsonIgnoreDecoration.Split(',').Any(c => c == col.Name);

            if (colFound)
            {
                decorator = $"{tab}{tab}[JsonIgnore]";
            }

            return decorator;
        }

        private string WriteMemberCustomDecorator(SettingsColumn settings)
        {
            var decorator = string.Empty;
            if (!string.IsNullOrEmpty(settings.FieldNameCustomDecorator))
            {
                decorator = _stringTransform.IndentString + _stringTransform.IndentString + settings.FieldNameCustomDecorator;
            }

            return decorator;
        }

        private static void SpaceBetweenDecorators(ref StringBuilder decorators, ref int curLength)
        {
            if (decorators.Length > curLength)
            {
                decorators.Append(Environment.NewLine);
                curLength = decorators.Length;
            }
        }
    }
}
