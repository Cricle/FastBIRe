namespace Diagnostics.Traces.Status
{
    public interface IStatusManager : IDisposable
    {
        IStatusStorageManager StatusStorageManager { get; }

        IAsyncEnumerable<StatusInfo> FindAsync(string name, DateTime? leftTime = null, DateTime? rightTime = null, CancellationToken token = default);

        IEnumerable<StatusInfo> Find(string name, DateTime? leftTime = null, DateTime? rightTime = null);

        Task<StatusInfo?> FindAsync(string name, string key, CancellationToken token = default);

        StatusInfo? Find(string name, string key);

        Task<IReadOnlyList<string>> GetNamesAsync(CancellationToken token = default);

        IReadOnlyList<string> GetNames();

        Task<bool> InitializeAsync(string name, CancellationToken token = default);

        bool Initialize(string name);

        Task<long?> CountAsync(string name, CancellationToken token = default);

        long? Count(string name);

        Task<long> CleanBeforeAsync(string name, DateTime time, CancellationToken token = default);

        Task<long> CleanAsync(string name, CancellationToken token = default);

        Task<IStatusScope> CreateScopeAsync(string name, CancellationToken token = default);

        IStatusScope CreateScope(string name);
    }
}