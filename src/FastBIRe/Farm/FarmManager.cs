using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Farm
{
    public static class FarmManagerFastExtensions
    {
        public static Task SyncDataAsync(this FarmManager manager, string tableName, int batchSize = FarmManager.DefaultBatchSize, CancellationToken token = default)
        {
            var sqlType = manager.SourceFarmWarehouse.SqlType;
            var tableColumns = manager.Columns.Select(x => sqlType.Wrap(x));
            var tableColumnJoined = string.Join(", ", tableColumns);
            return manager.SyncDataAsync($"SELECT {tableColumnJoined} FROM {sqlType.Wrap(tableName)}", tableName, batchSize, token);
        }
    }
    public class FarmManager : IFarmManager
    {
        public const int DefaultBatchSize = 1000;

        public static FarmManager Create(FarmWarehouse sourceFarmWarehouse, FarmWarehouse destFarmWarehouse, string sourceTable)
        {
            var table = sourceFarmWarehouse.DatabaseReader.Table(sourceTable);
            if (table == null)
            {
                throw new ArgumentException($"Table {sourceTable} not found!");
            }
            return new FarmManager(table, sourceFarmWarehouse, destFarmWarehouse);
        }

        public FarmManager(DatabaseTable sourceTable, FarmWarehouse sourceFarmWarehouse, FarmWarehouse destFarmWarehouse)
        {
            SourceTable = sourceTable;
            SourceFarmWarehouse = sourceFarmWarehouse;
            DestFarmWarehouse = destFarmWarehouse;
        }

        public DatabaseTable SourceTable { get; }

        public string TableName => SourceTable.Name;

        public IEnumerable<string> Columns => SourceTable.Columns.Select(x => x.Name);

        public FarmWarehouse SourceFarmWarehouse { get; }

        public FarmWarehouse DestFarmWarehouse { get; }

        public Task<SyncResult> SyncAsync(IEnumerable<int>? maskColumns = null, CancellationToken token = default)
        {
            return DestFarmWarehouse.SyncAsync(SourceTable, maskColumns, token);
        }

        public virtual Task InsertAsync(IEnumerable<IEnumerable<object?>> values, CancellationToken token = default)
        {
            return DestFarmWarehouse.InsertAsync(TableName, Columns, values, token);
        }
        public virtual Task InsertAsync(IEnumerable<object?> values, CancellationToken token = default)
        {
            return DestFarmWarehouse.InsertAsync(TableName, Columns, values, token);
        }
        public virtual Task<int> SyncDataAsync(string sql, string tableName, int batchSize = DefaultBatchSize, CancellationToken token = default)
        {
            return SourceFarmWarehouse.ScriptExecuter.ReadResultAsync(sql, (e, r) => DestFarmWarehouse.SyncDataAsync(tableName, r.Reader, batchSize, token), token: token);
        }
        public void Dispose()
        {
            SourceFarmWarehouse.Dispose();
            DestFarmWarehouse.Dispose();
        }
    }
}
