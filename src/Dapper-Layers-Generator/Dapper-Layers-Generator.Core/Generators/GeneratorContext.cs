using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorContext : IGenerator
    {

    }

    public class GeneratorContext: Generator, IGeneratorContext
    {
        public GeneratorContext(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService) 
            : base(settingsGlobal, data, stringTransformationService)
        {

        }


        public override string Generate()
        {
            return WriteContextClasses();
        }

        private string WriteContextClasses()
        {
            var builder = new StringBuilder();

            builder.Append(WriteContextHeaderComment());



            return builder.ToString();
        }

        private string WriteContextHeaderComment()
        {
            return $@"// =================================================================
// DBContext implements all repo management + a small context factory
// Author: {_settings.AuthorName}
// Context name: {_settings.DbContextClassName}
// Generated: {DateTime.Now}
// WARNING: Never change this file manually (re-generate it)
// =================================================================

namespace {_settings.TargetNamespaceForDbContext}
{{";

        }
    }
}
