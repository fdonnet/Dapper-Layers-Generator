﻿using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System.Text;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoGetByPkBulk : IGeneratorFromTable
    {

    }
    public class GeneratorRepoGetByPkBulk : GeneratorForOperations, IGeneratorRepoGetByPkBulk
    {

        public GeneratorRepoGetByPkBulk(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.GetByPkBulkGenerator && !string.IsNullOrEmpty(GetPkMemberNamesString()))
            {
                var output = new StringBuilder();
                output.Append(WriteMethodDef());
                if (!IsBase)
                {
                    //Will se if we can use some part of code for multi db providers for the moment the implementation is in MySql only child class
                }

                return output.ToString();
            }

            return string.Empty;
        }

        protected override string WriteMethodDef()
        {
            return $"{tab}{tab}//Please use this bulk by batch depending on the mem available 1000 / 1500 rows" + Environment.NewLine +
                $"{tab}{tab}public {(IsBase ? "abstract" : "override async")} Task<IEnumerable<{ClassName}>> GetBy{GetPkMemberNamesString()}BulkAsync({GetPkMemberNamesStringAndTypeList()})" +
                $"{(IsBase ? ";" : string.Empty)}" + (!IsBase ? @$"
{tab}{tab}{{" : Environment.NewLine);

        }

        protected override string WriteDapperCall()
        {
            return string.Empty;

        }

        protected override string WriteReturnObj()
        {
            return string.Empty;
        }
    }
}
