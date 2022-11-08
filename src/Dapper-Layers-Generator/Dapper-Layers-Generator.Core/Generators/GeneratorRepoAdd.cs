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
    public interface IGeneratorRepoAdd : IGeneratorFromTable
    {

    }
    public class GeneratorRepoAdd : GeneratorForOperations, IGeneratorRepoAdd
    {

        public GeneratorRepoAdd(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter)
                : base(settingsGlobal, data, stringTransformationService, dataConverter)
        {

        }
        public override string Generate()
        {
            if (TableSettings.AddGenerator)
            {
                var output = new StringBuilder();
                output.Append(GetMethodDef());
                output.Append(Environment.NewLine);
                output.Append(GetDapperDynaParams());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(@GetBaseSqlForInsert());
                output.Append($"{tab}{tab}{tab}{tab}" +@GetValuesToInsert());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetDapperCall());
                output.Append(Environment.NewLine);
                output.Append(Environment.NewLine);
                output.Append(GetReturnObj());
                output.Append(Environment.NewLine);
                output.Append($"{tab}{tab}}}");
                output.Append(Environment.NewLine);

                return output.ToString();
            }

            return string.Empty;
        }

        protected override string GetMethodDef()
        {
            return $"{tab}{tab}public {(IsBase ? "virtual" : "override")} async Task<{GetPkMemberTypes()}> AddAsync({_stringTransform.ApplyConfigTransformClass(ClassName)} " +
                    $"{_stringTransform.ApplyConfigTransformMember(ClassName)})" +
                @$"
{tab}{tab}{{";
        }

        protected override string GetDapperCall()
        {
            return $"{tab}{tab}{tab}_ = " +
                    $"await _{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Connection." +
                    $"ExecuteAsync(sql,p,transaction:_{_stringTransform.ApplyConfigTransformMember(_settings.DbContextClassName)}.Transaction);";
        }

        protected virtual string GetDapperDynaParams()
        {
            var output = new StringBuilder();
            output.Append($"{tab}{tab}{tab}var p = new DynamicParameters();");
            output.Append(Environment.NewLine);

            var cols = ColumnForInsertOperations!.Where(c => !c.IsAutoIncrement);

            var spParams = String.Join(Environment.NewLine, cols.OrderBy(c=>c.Position).Select(col =>
            {
                return $@"{tab}{tab}{tab}p.Add(""@{col.Name}"", {_stringTransform.ApplyConfigTransformMember(ClassName)}.{_stringTransform.PascalCase(col.Name)});";
            }));

            output.Append(spParams);
            return output.ToString();
        }


        protected virtual string GetValuesToInsert()
        {
            var output = new StringBuilder();

            var cols = ColumnForInsertOperations!.Where(c => !c.IsAutoIncrement);

            var values = String.Join(Environment.NewLine +$"{tab}{tab}{tab}{tab},", cols.OrderBy(c => c.Position).Select(col =>
            {
                return $@"@{col.Name}";
            }));


            output.Append(values);
            output.Append(Environment.NewLine);
            output.Append($@"{tab}{tab}{tab})"";");
            return output.ToString();
        }


        protected override string GetReturnObj()
        {
            //The base implementation is very minimal (no real return from the DB, need to be override by dbprovider specific)
            if (PkColumns.Count() > 1)
            {
                var returnValues = String.Join(", ", PkColumns
                    .Select(c => $"{_stringTransform.ApplyConfigTransformMember(ClassName)}.{_stringTransform.PascalCase(c.Name)}"));

                return $"{tab}{tab}{tab}return ({returnValues});";
            }
            else
            {
                var pk = PkColumns!.First();
                if (pk.IsAutoIncrement)
                {
                    //Need to implement return new autoincrement in dbprovder implementations
                    return $"{tab}{tab}{tab}return 1;";
                }
                else
                {
                    return $"{tab}{tab}{tab}return {_stringTransform.ApplyConfigTransformMember(ClassName)}.{_stringTransform.PascalCase(pk.Name)};";
                }
            }
        }
    }
}
