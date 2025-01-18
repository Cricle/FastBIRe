using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Querying
{
    public record class MergeQueryInsertRequest : MergeQueryRequest
    {
        public MergeQueryInsertRequest(FSqlType sqlType, DatabaseTable sourceTable, DatabaseTable destTable, IReadOnlyList<ITableFieldLink> noGroupLinks, IReadOnlyList<ITableFieldLink> groupLinks) : base(sqlType, sourceTable, destTable, noGroupLinks, groupLinks)
        {
        }
    }
}
