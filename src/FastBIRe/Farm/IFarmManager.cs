namespace FastBIRe.Farm
{
    public interface IFarmManager
    {
        Task SyncAsync(CancellationToken token=default);

        Task InsertAsync(IEnumerable<IEnumerable<object>> values, CancellationToken token = default);

        Task InsertAsync(IEnumerable<object> values, CancellationToken token = default);

        Task<IList<ICursorRowHandlerResult>> CheckPointAsync(CancellationToken token = default);
    }
}
