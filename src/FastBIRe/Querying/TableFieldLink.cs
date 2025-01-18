using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Querying
{
    public abstract record class TableFieldLink : ITableFieldLink
    {
        protected TableFieldLink(DatabaseColumn destColumn)
        {
            DestColumn = destColumn ?? throw new ArgumentNullException(nameof(destColumn));
        }
        public DatabaseColumn DestColumn { get; }

        public abstract string FormatExpression(FSqlType type, string? tableAlias);

        public abstract string FormatSql(FSqlType type, string? tableAlias);
    }
}
