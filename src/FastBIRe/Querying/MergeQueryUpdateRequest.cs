using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Querying
{
    public record class MergeQueryUpdateRequest : MergeQueryRequest
    {
        public MergeQueryUpdateRequest(SqlType sqlType, DatabaseTable sourceTable, DatabaseTable destTable, IReadOnlyList<ITableFieldLink> noGroupLinks, IReadOnlyList<ITableFieldLink> groupLinks) : base(sqlType, sourceTable, destTable, noGroupLinks, groupLinks)
        {
            IgnoreCompareFields = new List<string>();
        }

        public IList<string> IgnoreCompareFields { get; }
    }
}
