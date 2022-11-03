using Dapper_Layers_Generator.Core.Settings;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorPOCO : IGenerator, IGeneratorFromTable
    {

    }

    public class GeneratorPOCO : GeneratorFromTable , IGeneratorPOCO
    {
        public GeneratorPOCO(SettingsGlobal settingsGlobal, IReaderDBDefinitionService data)
            : base(settingsGlobal,data)
        {
            
        }

        public override string Generate()
        {
            if (Table == null)
                throw new NullReferenceException("Cannot use POCO generator without a loaded table (use SetTable)");


            string output =
$@" 
{WritePocoHeader()}";

            return output;


        }

        private string WritePocoHeader()
        {
            return $@" 
namespace {_settings.TargetNamespaceForPOCO} 
{{
/// =================================================================
/// Author: {_settings.AuthorName}
/// Description: Poco class for the table {Table.Name} 
/// =================================================================";
        }



    }
}
