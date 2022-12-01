using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorPOCO : IGeneratorFromTable
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

            return WritePocoClass();
        }

        private string WritePocoClass()
        {
            return
                $$"""
                {{WritePocoHeaderComment()}}
                {{tab}}public class {{ClassName}}
                {{tab}}{
                {{WritePocoMemberFields()}}
                {{tab}}}
                }
                """;
        }

        private string WritePocoHeaderComment()
        {
            return
                $$"""
                #nullable disable warnings
                namespace {{_settings.TargetNamespaceForPOCO}} 
                {
                {{tab}}/// =================================================================
                {{tab}}/// <summary>
                {{tab}}/// Poco class for the table {{Table.Name}}
                {{tab}}/// Author: {{_settings.AuthorName}}
                {{tab}}/// Poco: {{ClassName}}
                {{tab}}/// Generated: {{_settings.GenerationTimestamp.ToString("yyyy-MM-dd HH:mm:ss")}} UTC
                {{tab}}/// WARNING: Never change this file manually (re-generate it)
                {{tab}}/// </summary>
                {{tab}}/// =================================================================
                """;
        }

        private string WritePocoMemberFields()
        {
            if (Table.Columns == null)
                return "";

            var columns = string.IsNullOrEmpty(TableSettings.IgnoredColumnNames)
                          ? Table.Columns
                          : Table.Columns.Where(c => !TableSettings.IgnoredColumnNames.Split(',').Contains(c.Name));

            var members = String.Join(Environment.NewLine + Environment.NewLine, columns.Select(col =>
            {
                var memberName = _stringTransform.PascalCase(col.Name);
                var colSettings = TableSettings.GetColumnSettings(col.Name);
                var memberType = GetColumnDotNetType(col);

                var decorators = WriteMemberDecorators(colSettings, memberType, col);

                return $"{(string.IsNullOrEmpty(decorators)
                    ? string.Empty
                    : decorators + Environment.NewLine)}{tab}{tab}public {memberType} {memberName} {{ get; set; }}";

            }));

            return members;
        }

        private string WriteMemberDecorators(SettingsColumn settings, string memberType, Column col)
        {
            var decorators = new List<string>
            {
                WriteMemberRequieredDecorator(settings, col),
                WriteMemberStringDecorator(settings, memberType, col),
                WriteMemberJsonIgnoreDecorator(col),
                WriteMemberCustomDecorator(settings)
            };

            return String.Join(Environment.NewLine, decorators.Where(d => !string.IsNullOrEmpty(d)));
        }

        private string WriteMemberStringDecorator(SettingsColumn settings, string memberType, Column col)
        {
            var decorator = string.Empty;

            if (memberType == "string" || memberType == "string?")
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

        private string WriteMemberRequieredDecorator(SettingsColumn settings, Column col)
        {
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

        private string WriteMemberJsonIgnoreDecorator(Column col)
        {
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
    }
}
