using System.Data;

namespace FastBIRe
{
    public interface IDataSchemaDataTable:IDisposable
    {
        DataSchema DataSchema { get; }
        int RowCount { get; }
        int Count { get; }

        object? this[int index] { get; }
        object? this[int row, int column] { get; }

        object?[] GetRowBuffer();
        void Append(object? item);
        void Append(ReadOnlySpan<object?> source);
        Span<object?> GetRow(int index);
    }
    public static class DataSchemaDataTableExtensions
    {
        public static void AppendTable(this IDataSchemaDataTable table, IDataReader reader)
        {
            var buffer = table.GetRowBuffer();
            while (reader.Read())
            {
                var size = reader.GetValues(buffer!);
                table.Append(buffer.AsSpan(0, size));
            }
        }
        public static void AppendRow(this IDataSchemaDataTable table, IDataRecord record)
        {
            var buff = table.GetRowBuffer();
            var size = record.GetValues(buff!);
            table.Append(buff.AsSpan(0, size));
        }
        public static object? GetRowColumn(this IDataSchemaDataTable table,int index, int columnIndex)
        {
            return table.GetRow(index)[columnIndex];
        }
        public static T? GetRowColumn<T>(this IDataSchemaDataTable table, int index, int columnIndex)
        {
            var val = GetRowColumn(table, index, columnIndex);
            return TypeVisibility<T>.ChangeType(val);
        }

        public static object? GetRowColumn(this IDataSchemaDataTable table, int index, string columnName)
        {
            var colIndex = FindColumnIndex(table,columnName);
            if (colIndex == -1)
            {
                throw new ArgumentException($"No such column {columnName}");
            }
            return GetRowColumn(table,index, colIndex);
        }
        public static T? GetRowColumn<T>(this IDataSchemaDataTable table, int index, string columnName)
        {
            var val = GetRowColumn(table, index, columnName);
            return TypeVisibility<T>.ChangeType(val);
        }
        private static int FindColumnIndex(IDataSchemaDataTable table, string columnName)
        {
            for (int i = 0; i < table.DataSchema.Names.Count; i++)
            {
                if (table.DataSchema.Names[i] == columnName)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}