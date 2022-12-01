using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Generators
{

    public interface IGeneratorRepoDelete : IGeneratorFromTable
    {

    }
    public class GeneratorRepoDelete : GeneratorForOperations, IGeneratorRepoDelete
    {
        public GeneratorRepoDelete(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }

        public override string Generate()
        {
            if (TableSettings.GetByPkGenerator)
            {
                if (!PkColumns.Any())
                    throw new ArgumentException($"You cannot run the Get by Pk Generator for table {Table.Name}, no pk defined");

                var output = new StringBuilder();
                output.Append(WriteMethodDef());
                output.Append(Environment.NewLine);
                output.Append(@GetDapperDynaParamsForPk());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetBaseSqlForDelete());
                output.Append(Environment.NewLine);
                output.Append(GetSqlWhereClauseForPk());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(WriteDapperCall());
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}}}");
                output.Append(Environment.NewLine);

                return output.ToString();
            }

            return string.Empty;
        }
        
        protected override string WriteMethodDef()
        {
            return $"{tab}{tab}public {(IsBase ? "virtual" : "override")} async Task DeleteAsync({GetPkMemberNamesStringAndType()})" +
                @$"
{tab}{tab}{{";
        }

        protected override string WriteDapperCall()
        {
            return $"{tab}{tab}{tab}_ = await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected override string WriteReturnObj()
        {
            return string.Empty;
        }
    }
}
