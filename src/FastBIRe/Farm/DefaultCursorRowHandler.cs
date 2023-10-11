using DatabaseSchemaReader;
using FastBIRe.Data;
using System.Data;

namespace FastBIRe.Farm
{
    public class DefaultCursorRowHandler : ICursorRowHandler
    {
        public static DefaultCursorRowHandler FromDefault(IDbScriptExecuter sourceConnection, IDbScriptExecuter destConnection, string tableName)
        {
            return new DefaultCursorRowHandler(sourceConnection, destConnection, tableName, FarmWarehouse.CursorTable, FarmWarehouse.SyncPoint, FarmWarehouse.SyncNamer, FarmWarehouse.Id);
        }

        public DefaultCursorRowHandler(IDbScriptExecuter sourceConnection, IDbScriptExecuter destConnection, string tableName, string cursorTable, string pointColumn, string nameColumn, string idColumn)
        {
            SourceConnection = sourceConnection;
            DestConnection = destConnection;
            TableName = tableName;
            CursorTable = cursorTable;
            PointColumn = pointColumn;
            NameColumn = nameColumn;
            IdColumn = idColumn;
        }

        public IDbScriptExecuter SourceConnection { get; }

        public IDbScriptExecuter DestConnection { get; }

        public string TableName { get; set; }

        public string CursorTable { get; }

        public string PointColumn { get; }

        public string NameColumn { get; }

        public string IdColumn { get; }

        public int QueryBatchSize { get; set; } = 1000;

        public int CopyBatchSize { get; set; } = 1000;

        public async Task HandlerCursorRowAsync(CursorRow rows, CancellationToken token = default)
        {
            var sourceReader = new DatabaseReader(SourceConnection.Connection);
            var destReader = new DatabaseReader(DestConnection.Connection);

            var sourceTable = sourceReader.Table(TableName);

            var sourceSqlType = sourceReader.SqlType!.Value;
            var destSqlType = destReader.SqlType!.Value;

            var destTableHelper = new TableHelper(destSqlType);

            var target = new SQLMirrorTarget(SourceConnection, sourceSqlType.Wrap(TableName));
            var escaper = sourceSqlType.GetMethodWrapper();
            var includeNames= new HashSet<string>(sourceTable.Columns.Select(x => x.Name));
            var currentPoint = rows.Point;
            while (true)
            {
                using (var trans = DestConnection.Connection.BeginTransaction())
                {
                    ulong maxId = 0;
                    var queryDataSql = $"SELECT {destSqlType.Wrap(IdColumn)},{string.Join(",", sourceTable.Columns.Select(x => destSqlType.Wrap(x.Name)))} FROM {destSqlType.Wrap(TableName)} WHERE {destSqlType.Wrap(IdColumn)} > {currentPoint} {destTableHelper.Pagging(null, QueryBatchSize)}";
                    await DestConnection.ReadAsync(queryDataSql, async (s, e) =>
                    {
                        var cop = new SQLMirrorCopy(e.Reader, target, escaper, CopyBatchSize);
                        cop.IncludeNames = includeNames;
                        cop.DataCapturer = new FieldDataCapture(IdColumn, o =>
                        {
                            var id = Convert.ToUInt64(o);
                            if (id > maxId)
                            {
                                maxId = id;
                            }
                        });
                        await cop.CopyAsync();
                    }, token);
                    currentPoint = maxId;
                    if (maxId != 0)
                    {
                        var updateSql = $"UPDATE {destSqlType.Wrap(CursorTable)} SET {destSqlType.Wrap(PointColumn)} = {maxId} WHERE {destSqlType.Wrap(NameColumn)} = {destSqlType.WrapValue(rows.Name)}";
                        await DestConnection.ExecuteAsync(updateSql, token);
                    }
                    trans.Commit();
                    if (maxId == 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}
