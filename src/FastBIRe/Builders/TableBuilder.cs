using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Builders
{
    public class TableBuilder : ITableBuilder
    {
        public TableBuilder(DatabaseTable table, SqlType sqlType)
        {
            Table = table;
            SqlType = sqlType;
        }

        public string Name => Table.Name;

        public DatabaseTable Table { get; }

        public SqlType SqlType { get; }

        public ITableBuilder Config(Action<DatabaseTable> configuration)
        {
            configuration(Table);
            return this;
        }

        public ITableColumnBuilder GetColumnBuilder(string name)
        {
            var column = Table.FindColumn(name);
            if (column != null)
            {
                return new TableColumnBuilder(Table, column, SqlType);
            }
            column = new DatabaseColumn { Name = name };
            Table.AddColumn(column);
            return new TableColumnBuilder(Table, column, SqlType);
        }
    }
}
