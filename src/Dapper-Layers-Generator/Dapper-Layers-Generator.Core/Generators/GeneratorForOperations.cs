using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public abstract class GeneratorForOperations : GeneratorFromTable
    {
        public GeneratorForOperations(SettingsGlobal settingsGlobal
           , IReaderDBDefinitionService data
           , StringTransformationService stringTransformationService
           , IDataTypeConverter dataConverter)
               : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {
            
        }




        protected abstract string GetMethodDef();
        protected abstract string GetDapperCall();
    }
}
