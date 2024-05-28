namespace FastBIRe.Farm
{
    public interface IFarmManager : IDisposable
    {
        Task<SyncResult> SyncAsync(IEnumerable<int>? maskColumns = null, CancellationToken token = default);

        Task InsertAsync(IEnumerable<IEnumerable<object>> values, CancellationToken token = default);

        Task InsertAsync(IEnumerable<object> values, CancellationToken token = default);

        Task<int> SyncDataAsync(string sql, string tableName, int batchSize, CancellationToken token = default);
    }
}
