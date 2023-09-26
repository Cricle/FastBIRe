using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Querying
{
    public abstract record class TableFieldLink: ITableFieldLink
    {
        protected TableFieldLink(DatabaseColumn destColumn)
        {
            DestColumn = destColumn ?? throw new ArgumentNullException(nameof(destColumn));
        }
        public DatabaseColumn DestColumn { get; }

        public abstract string FormatExpression(SqlType type, string? tableAlias);

        public abstract string FormatSql(SqlType type, string? tableAlias);
    }
}
