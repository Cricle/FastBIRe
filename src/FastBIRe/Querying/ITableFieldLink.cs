using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Querying
{
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
