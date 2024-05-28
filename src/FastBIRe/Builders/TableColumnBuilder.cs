using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Builders
{
    public class TableColumnBuilder : ITableColumnBuilder
    {
        public TableColumnBuilder(DatabaseTable table, DatabaseColumn column, SqlType sqlType)
        {
            Table = table;
            Column = column;
            SqlType = sqlType;
        }

        public DatabaseTable Table { get; }

        public DatabaseColumn Column { get; }

        public SqlType SqlType { get; }

        public ITableColumnBuilder Config(Action<DatabaseColumn> configuration)
        {
            configuration(Column);
            return this;
        }
    }
}
