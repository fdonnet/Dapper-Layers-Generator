using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorRepoGetByPk : IGeneratorFromTable
    {

    }

    public class GeneratorRepoGetByPk : GeneratorForOperations, IGeneratorRepoGetAll
    {
        public GeneratorRepoGetByPk(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.GetAllGenerator)
            {
                var output = new StringBuilder();
                output.Append(GetMethodDef());
                output.Append(Environment.NewLine);
                output.Append(@GetBaseSql());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetDapperCall());

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

        protected override string GetMethodDef()
        {
            return $"{tab}{tab}public {(IsBase ? "virtual" : "override")} async Task<{ClassName}> GetBy{GetPkMemberNamesString()}Async({GetPkMemberNamesStringAndType()})" +
                @$"
{tab}{tab}{{";
        }

        protected override string GetDapperCall()
        {
            return $"{tab}{tab}{tab}var {_stringTransform.PluralizeToLower(ClassName)} = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"QueryAsync<{ClassName}>(sql,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }



    }
}
