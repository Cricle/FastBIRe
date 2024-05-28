using DatabaseSchemaReader.DataSchema;
using FastBIRe.Timing;

namespace FastBIRe.Querying
{
    public record class ExpandTableFieldLink : TableFieldLink
    {
        public ExpandTableFieldLink(DatabaseColumn destColumn, IExpandResult expandResult)
            : base(destColumn)
        {
            ExpandResult = expandResult;
        }
        /// <summary>
        /// The link expand result
        /// </summary>
        public IExpandResult ExpandResult { get; }

        public override string FormatExpression(SqlType type, string? tableAlias)
        {
            string? formatExp;
            if (string.IsNullOrWhiteSpace(tableAlias))
            {
                formatExp = type.Wrap(ExpandResult.Name);
            }
            else
            {
                formatExp = $"{type.Wrap(tableAlias)}.{type.Wrap(ExpandResult.Name)}";
            }
            return $"({ExpandResult.FormatExpression(formatExp)})";
        }

        /// <inheritdoc/>
        public override string FormatSql(SqlType type, string? tableAlias)
        {
            return $"{FormatExpression(type, tableAlias)} AS {type.Wrap(DestColumn.Name)}";
        }
    }
}
