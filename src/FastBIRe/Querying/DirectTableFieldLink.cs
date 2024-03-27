using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Querying
{
    public record class DirectTableFieldLink : TableFieldLink
    {
        public DirectTableFieldLink(DatabaseColumn destColumn, DatabaseColumn sourceColumn)
            : base(destColumn)
        {
            SourceColumn = sourceColumn;
        }
        /// <summary>
        /// The link source column
        /// </summary>
        public DatabaseColumn SourceColumn { get; }

        public override string FormatExpression(SqlType type, string? tableAlias)
        {
            var aliasExp = string.Empty;
            if (!string.IsNullOrEmpty(tableAlias))
            {
                aliasExp = $"{type.Wrap(tableAlias)}.";
            }
            return $"{aliasExp}{type.Wrap(SourceColumn.Name)}";
        }

        public override string FormatSql(SqlType type, string? tableAlias)
        {
            return $"{FormatExpression(type, tableAlias)} AS {type.Wrap(DestColumn.Name)}";
        }
    }
}
