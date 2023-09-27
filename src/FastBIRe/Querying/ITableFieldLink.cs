using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.Timing;

namespace FastBIRe.Querying
{
    public class TableFieldLinkBuilder
    {
        public TableFieldLinkBuilder(DatabaseTable sourceTable, DatabaseTable destTable)
        {
            SourceTable = sourceTable ?? throw new ArgumentNullException(nameof(sourceTable));
            DestTable = destTable ?? throw new ArgumentNullException(nameof(destTable));
        }

        public DatabaseTable SourceTable { get; }

        public DatabaseTable DestTable { get; }

        public ITableFieldLink Direct(string sourceFieldName, string destFieldName)
        {
            var sourceField = SourceTable.FindColumn(sourceFieldName);
            if (sourceField == null)
                Throws.ThrowFieldNotFound(sourceFieldName, SourceTable.Name);
            var destField = DestTable.FindColumn(destFieldName);
            if (destField == null)
                Throws.ThrowFieldNotFound(destFieldName, DestTable.Name);
            return new DirectTableFieldLink(destField!, sourceField!);
        }
        public ITableFieldLink Expand(string destFieldName,IExpandResult expandResult)
        {
            var destField = DestTable.FindColumn(destFieldName);
            if (destField == null)
                Throws.ThrowFieldNotFound(destFieldName, DestTable.Name);
            return new ExpandTableFieldLink(destField!, expandResult);
        }
        public static TableFieldLinkBuilder From(DatabaseReader reader,string sourceTableName,string destTableName)
        {
            var sourceTable = reader.Table(sourceTableName);
            if (sourceTable == null)
                Throws.ThrowTableNotFound(sourceTableName);
            var destTable = reader.Table(destTableName);
            if (destTable == null)
                Throws.ThrowTableNotFound(destTableName);
            return new TableFieldLinkBuilder(sourceTable!, destTable!);
        }
    }
    public interface ITableFieldLink
    {
        /// <summary>
        /// The dest column
        /// </summary>
        DatabaseColumn DestColumn { get; }

        /// <summary>
        /// Format the link to sql unit
        /// </summary>
        /// <param name="type">The sql type</param>
        /// <param name="tableAlias">The source table alias, if <see langword="null"/> will generate nothing for that</param>
        /// <returns>The result sql</returns>
        string FormatSql(SqlType type, string? tableAlias);
        /// <summary>
        /// Format expression
        /// </summary>
        /// <param name="type">The sql type</param>
        /// <param name="tableAlias">The source table alias, if <see langword="null"/> will generate nothing for that</param>
        /// <returns></returns>
        string FormatExpression(SqlType type, string? tableAlias);
    }
}
