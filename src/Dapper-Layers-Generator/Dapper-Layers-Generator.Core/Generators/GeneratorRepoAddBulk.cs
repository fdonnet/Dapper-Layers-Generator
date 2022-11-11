using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoAddBulk : IGeneratorFromTable
    {

    }
    public class GeneratorRepoAddBulk : GeneratorForOperations, IGeneratorRepoAddBulk
    {

        public GeneratorRepoAddBulk(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.AddBulkGenerator)
            {
                var output = new StringBuilder();
                output.Append(GetMethodDef());
                if (!IsBase)
                {
                    //Will se if we can use some part of code for multi db providers for the moment the implementation is in MySql only child class
                }

                return output.ToString();
            }

            return string.Empty;
        }

        protected override string GetMethodDef()
        {
            return $"{tab}{tab}//Please use this bulk by batch depending on the mem available 500 / 1000 / 1500 rows" + Environment.NewLine +
                $"{tab}{tab}public {(IsBase ? "abstract" : "override async")} Task AddBulkAsync(IEnumerable<{ClassName}> " +
            $"{_stringTransform.PluralizeToLower(ClassName)}){(IsBase ? ";" : string.Empty)}" + (!IsBase ? @$"
{tab}{tab}{{" : Environment.NewLine);

        }

        protected override string GetDapperCall()
        {
            return string.Empty;

        }

        protected override string GetReturnObj()
        {
            return string.Empty;
        }
    }
}
