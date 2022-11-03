using Dapper_Layers_Generator.Core.Settings;
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
            , StringTransformationService stringTransformationService)
                : base(settingsGlobal, data, stringTransformationService)
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
            return $@" 
namespace {_settings.TargetNamespaceForPOCO} 
{{
/// =================================================================
/// Author: {_settings.AuthorName}
/// Poco: {ClassName}
/// Description: Poco class for the table {Table.Name}
/// Generated: {DateTime.Now}
/// =================================================================";
        }

        private string WritePocoClass()
        {
            return $@" 
{WritePocoHeaderComment()}

    public class {ClassName}
    {{

    }}
}}";

        }

        private string WritePocoMemberFields()
        {
            if (Table.Columns == null)
                return "";

            var columns = Table.Columns;

            var members = String.Join(Environment.NewLine + "       ", columns.Select(col =>
            {
                var memberName = _stringTransform.PascalCase(col.Name);
                var colSettings = TableSettings.GetColumnSettings(col.Name);


                

                return $"" + Environment.NewLine;
            }));
        }

        private string WriteMemberDecorators(SettingsColumn settings)
        {
            var decorator = string.Empty;

            if(settings.StandardStringLengthDecorator)
            {

            }
        }


    }
}
