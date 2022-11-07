using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;

namespace Dapper_Layers_Generator.Core.Generators
{
    public interface IGeneratorFromTable : IGenerator
    {   string ClassName { get; }
        void SetTable(string tableName);
    }

    public abstract class GeneratorFromTable : Generator, IGeneratorFromTable
    {
        protected ITable Table { get; private set; } = null!;
        public string ClassName { get; private set; } = null!;
        protected SettingsTable TableSettings { get; private set; } = null!;
        protected IDataTypeConverter DataConverter { get; private set; } = null!;
        protected IEnumerable<IColumn> PkColumns { get; private set; } = Enumerable.Empty<IColumn>();
        protected IEnumerable<IColumn> UkColumns { get; private set; } = Enumerable.Empty<IColumn>();
        protected Dictionary<string,List<IColumn>> ColumnNamesByIndexNameDic { get; private set; }
            = new Dictionary<string, List<IColumn>>();

        public GeneratorFromTable(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter) 
                : base(settingsGlobal, data, stringTransformationService)
        {
            DataConverter = dataConverter;
        }

        public override abstract string Generate();

        public void SetTable(string tableName)
        {
            var table = _currentSchema.Tables?.Where(t => t.Name == tableName).SingleOrDefault();

            if (table == null)
                throw new NullReferenceException("Table not found in the DB repository");

            Table = table;
            ClassName = _stringTransform.ApplyConfigTransformClass(Table.Name)!;
            TableSettings = _settings.GetTableSettings(Table.Name);

            PkColumns = Table.Columns?.Where(t => t.IsPrimary).ToList() 
                ?? Enumerable.Empty<IColumn>();

            UkColumns = Table.Columns?.Where(t => t.UniqueIndexNames != null && t.UniqueIndexNames.Any()).ToList() 
                ?? Enumerable.Empty<IColumn>();

            //Clear if generator not purelly scoped
            ColumnNamesByIndexNameDic = new Dictionary<string, List<IColumn>>();
            foreach (var col in UkColumns)
            {
                foreach(var index in col.UniqueIndexNames!)
                {
                    if(ColumnNamesByIndexNameDic.TryGetValue(index, out var currentList))
                    {
                        currentList.Add(col);
                    }
                    else
                    {
                        ColumnNamesByIndexNameDic.Add(index, new List<IColumn>() {col});
                    }
                }
            }
        }

        protected string GetPkMemberNamesString()
        {
            return string.Join("And", PkColumns.Select(c => _stringTransform.ApplyConfigTransformClass(c.Name)));
        }


        protected string GetUkMemberNamesString(string indexName)
        {
            if (ColumnNamesByIndexNameDic.TryGetValue(indexName, out var curIndex))
            {
                return string.Join("And", curIndex.OrderBy(c => c.Position).Select(c=> _stringTransform.ApplyConfigTransformClass(c.Name)));
            }
            else
                throw new NullReferenceException($"Cannot find the specified index {indexName} for table {Table.Name}");
        }

        protected string GetUkMemberNamesStringAndType(string indexName)
        {
            if (ColumnNamesByIndexNameDic.TryGetValue(indexName, out var curIndex))
            {
                return string.Join(", ", curIndex.OrderBy(c => c.Position).Select(c => $"{GetColumnDotNetType(c)} {_stringTransform.ApplyConfigTransformMember(c.Name)}"));
            }
            else
                throw new NullReferenceException($"Cannot find the specified index {indexName} for table {Table.Name}");
        }

        protected string GetPkMemberNamesStringAndType()
        {
            return string.Join(", ", PkColumns.Select(c => 
                $"{GetColumnDotNetType(c)} {_stringTransform.ApplyConfigTransformMember(c.Name)}"));

        }

        protected string GetPkMemberTypes()
        {
            var output = string.Join(", ", PkColumns.Select(c =>
                $"{GetColumnDotNetType(c)}"));

            if (PkColumns.Count() > 1)
                return $"({output})";
            else
                return output;
        }

        protected string GetPkMemberNamesStringAndTypeList()
        {
            var output = string.Empty;
            if(PkColumns.Count()>1)
            {
                var tuple = string.Join(", ", PkColumns.Select(c => $"{GetColumnDotNetType(c)}"));
                var varName = string.Join("And",PkColumns.Select(c => _stringTransform.ApplyConfigTransformClass(c.Name)));
                output = $"IEnumerable<({tuple})> listOf{varName}";
            }
            else
            {
                output = PkColumns.Select(c => $"IEnumerable<{GetColumnDotNetType(c)}> listOf{_stringTransform.ApplyConfigTransformClass(c.Name)}").First();
            }
            
            return output;
        }

        protected string GetColumnDotNetType(IColumn column)
        {
            var colSettings = TableSettings.GetColumnSettings(column.Name);

            var memberType = colSettings.FieldNameCustomType == String.Empty
                                ? DataConverter.GetDotNetDataType(column.DataType, column.IsNullable)
                                : colSettings.FieldNameCustomType;

            return memberType;

        }

    }
}
