using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            return $@"#nullable disable warnings
namespace {_settings.TargetNamespaceForPOCO} 
{{
{_stringTransform.IndentString}/// =================================================================
{_stringTransform.IndentString}/// <summary>
{_stringTransform.IndentString}/// Poco class for the table {Table.Name}
{_stringTransform.IndentString}/// Author: {_settings.AuthorName}
{_stringTransform.IndentString}/// Poco: {ClassName}
{_stringTransform.IndentString}/// Generated: {DateTime.Now}
{_stringTransform.IndentString}/// WARNING: Never change this file manually (re-generate it)
{_stringTransform.IndentString}/// </summary>
{_stringTransform.IndentString}/// =================================================================";

        }

        private string WritePocoClass()
        {
            var builder = new StringBuilder();
            
            builder.Append(WritePocoHeaderComment());
            
            builder.Append(Environment.NewLine);
            builder.Append($"{_stringTransform.IndentString}public class {ClassName}");
            builder.Append(Environment.NewLine);
            builder.Append($"{_stringTransform.IndentString}{{");
            builder.Append(Environment.NewLine);

            builder.Append(WritePocoMemberFields());

            builder.Append($"{_stringTransform.IndentString}}}");
            builder.Append(Environment.NewLine);
            builder.Append("}");

            return builder.ToString();
            
        }

        private string WritePocoMemberFields()
        {
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

                return  $"{decorators}{_stringTransform.IndentString}{_stringTransform.IndentString}public {memberType} {memberName} {{ get; set; }}"
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
            var decorator = string.Empty;

            if (memberType == "string")
            {
                if (settings.StandardStringLengthDecorator)
                {
                    if (col.Length > 0)
                    {
                        decorator = $"{_stringTransform.IndentString}{_stringTransform.IndentString}[System.ComponentModel.DataAnnotations.StringLength({col.Length})]";
                    }
                }
            }
            return decorator;
        }

        private string WriteMemberRequieredDecorator(SettingsColumn settings, IColumn col)
        {
            var decorator = string.Empty;

            if (!col.IsNullable && !col.IsAutoIncrement)
            {
                if (settings.StandardRequiredDecorator)
                {
                    decorator = $"{_stringTransform.IndentString}{_stringTransform.IndentString}[System.ComponentModel.DataAnnotations.Required]";
                }
            }
            return decorator;
        }

        private string WriteMemberJsonIgnoreDecorator(IColumn col)
        {
            var decorator = string.Empty;
            var colFound = TableSettings.JsonIgnoreDecoration.Split(',').Any(c => c == col.Name);

            if (colFound)
            {
                decorator = $"{_stringTransform.IndentString}{_stringTransform.IndentString}[JsonIgnore]";
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
