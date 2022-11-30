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
        protected Table Table { get; private set; } = null!;
        public string ClassName { get; private set; } = null!;
        protected SettingsTable TableSettings { get; private set; } = null!;
        protected IDataTypeConverter DataConverter { get; private set; } = null!;
        protected IEnumerable<Column> PkColumns { get; private set; } = Enumerable.Empty<Column>();
        protected IEnumerable<Column> UkColumns { get; private set; } = Enumerable.Empty<Column>();
        protected Dictionary<string,List<Column>> ColumnNamesByIndexNameDic { get; private set; }
            = new Dictionary<string, List<Column>>();

        protected virtual string ColAndTableIdentifier { get; init; } = String.Empty;
        protected virtual bool IsBase { get; init; } = true;

        protected IEnumerable<Column>? ColumnForGetOperations;
        protected IEnumerable<Column>? ColumnForInsertOperations;
        protected IEnumerable<Column>? ColumnForUpdateOperations;


        public GeneratorFromTable(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService
            , IDataTypeConverter dataConverter) 
                : base(settingsGlobal, data, stringTransformationService)
        {
            DataConverter = dataConverter;

        }

        public override abstract string Generate();

        public virtual void SetTable(string tableName)
        {
            var table = _currentSchema.Tables?.Where(t => t.Name == tableName).SingleOrDefault();

            if (table == null)
                throw new NullReferenceException("Table not found in the DB repository");

            Table = table;
            ClassName = _stringTransform.ApplyConfigTransformClass(Table.Name)!;
            TableSettings = _settings.GetTableSettings(Table.Name);

            PkColumns = Table.Columns?.Where(t => t.IsPrimary).ToList() 
                ?? Enumerable.Empty<Column>();

            UkColumns = Table.Columns?.Where(t => t.UniqueIndexNames != null && t.UniqueIndexNames.Any()).ToList() 
                ?? Enumerable.Empty<Column>();

            //Clear if generator not purelly scoped
            ColumnNamesByIndexNameDic = new Dictionary<string, List<Column>>();
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
                        ColumnNamesByIndexNameDic.Add(index, new List<Column>() {col});
                    }
                }
            }

            ColumnForGetOperations = Table.Columns != null
                ? Table.Columns.Where(c => !TableSettings.IgnoredColumnNames.Split(',').Contains(c.Name) && !TableSettings.IgnoredColumnNamesForGet.Split(',').Contains(c.Name))
                : throw new ArgumentException($"No column available for this table{Table.Name}, genererator crash");

            ColumnForInsertOperations = Table.Columns != null
                ? Table.Columns.Where(c => !TableSettings.IgnoredColumnNames.Split(',').Contains(c.Name) && !TableSettings.IgnoredColumnNamesForAdd.Split(',').Contains(c.Name))
                : throw new ArgumentException($"No column available for this table{Table.Name}, genererator crash");

            ColumnForUpdateOperations = Table.Columns != null
                ? Table.Columns.Where(c => !TableSettings.IgnoredColumnNames.Split(',').Contains(c.Name) && !TableSettings.IgnoredColumnNamesForUpdate.Split(',').Contains(c.Name))
                : throw new ArgumentException($"No column available for this table{Table.Name}, genererator crash");
        }

        protected string GetPkMemberNamesString()
        {
            return string.Join("And", PkColumns.Select(c => _stringTransform.ApplyConfigTransformClass(c.Name)));
        }


        protected string GetUkMemberNamesString(string indexName)
        {
            return ColumnNamesByIndexNameDic.TryGetValue(indexName, out var curIndex)
                ? string.Join("And", curIndex.OrderBy(c => c.Position).Select(c =>
                    _stringTransform.ApplyConfigTransformClass(c.Name)))
                : throw new NullReferenceException(
                    $"Cannot find the specified index {indexName} for table {Table.Name}");
        }

        protected string GetUkMemberNamesStringAndType(string indexName)
        {
            return ColumnNamesByIndexNameDic.TryGetValue(indexName, out var curIndex)
                ? string.Join(", ", curIndex.OrderBy(c => c.Position).Select(c =>
                    $"{GetColumnDotNetType(c)} {_stringTransform.ApplyConfigTransformMember(c.Name)}"))
                : throw new NullReferenceException(
                    $"Cannot find the specified index {indexName} for table {Table.Name}");
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

            return PkColumns.Count() > 1 ? $"({output})" : output;
        }

        protected string GetPkMemberNamesStringAndTypeList()
        {
            var output = string.Empty;
            if (PkColumns.Count() > 1)
            {
                var tuple = string.Join(", ", PkColumns.Select(c => $"{GetColumnDotNetType(c)}"));
                var varName = string.Join("And", PkColumns.Select(c => _stringTransform.ApplyConfigTransformClass(c.Name)));
                output = $"IEnumerable<({tuple})> listOf{varName}";
            }
            else
            {
                output = PkColumns.Select(c => $"IEnumerable<{GetColumnDotNetType(c)}> " +
                    $"listOf{_stringTransform.ApplyConfigTransformClass(c.Name)}").First();

            }

            return output;
        }

        protected string GetPKMemberNamesStringList()
        {
            string varName = string.Empty;
            if (PkColumns.Count() > 1)
            {
                varName = string.Join("And", PkColumns.Select(c => _stringTransform.ApplyConfigTransformClass(c.Name)));
            }
            else
                varName = PkColumns.Select(c => _stringTransform.ApplyConfigTransformClass(c.Name)).First()!;

            return "listOf" + varName;

        }

        protected string GetColumnDotNetType(Column column)
        {
            var colSettings = TableSettings.GetColumnSettings(column.Name);

            var memberType = colSettings.FieldNameCustomType == String.Empty
                                ? DataConverter.GetDotNetDataType(column.DataType, column.IsNullable)
                                : colSettings.FieldNameCustomType;

            return memberType;

        }

    }
}
