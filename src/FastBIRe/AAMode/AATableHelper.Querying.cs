using FastBIRe.Querying;

namespace FastBIRe.AAMode
{
    public partial class AATableHelper
    {
        public string QueryInsert(MergeQuerying mergeQuerying, string destTableName, IReadOnlyList<ITableFieldLink> noGroupLinks, IReadOnlyList<ITableFieldLink> groupLinks,Func<MergeQueryInsertRequest, MergeQueryInsertRequest>? requestFun=null)
        {
            var destTable = DatabaseReader.Table(destTableName);
            if (destTable == null)
                Throws.ThrowTableNotFound(destTableName);
            var request = new MergeQueryInsertRequest(SqlType, Table, destTable!, noGroupLinks, groupLinks);
            request = requestFun?.Invoke(request) ?? request;
            return mergeQuerying.Insert(request);
        }
    }
}
