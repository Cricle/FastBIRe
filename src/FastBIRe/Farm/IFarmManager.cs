namespace FastBIRe.Farm
{
    public interface IFarmManager : IDisposable
    {
        Task SyncAsync(IEnumerable<int>? maskColumns, CancellationToken token = default);

        Task InsertAsync(IEnumerable<IEnumerable<object>> values, CancellationToken token = default);

        Task InsertAsync(IEnumerable<object> values, CancellationToken token = default);
    }
}
