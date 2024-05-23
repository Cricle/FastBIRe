namespace Diagnostics.Traces.Status
{
    public interface IStatusManager
    {
        int Count { get; }

        Task<IReadOnlyList<string>> GetNamesAsync(CancellationToken token = default);

        Task<bool> NameExistsAsync(string name, CancellationToken token = default);

        Task<IStatusInstance> GetInstanceAsync(string name, CancellationToken token = default);
    }
    public interface IStatusInstance : IDisposable
    {
        string Name { get; }

        Task<long> CountAsync(CancellationToken token = default);

        Task GetAsync(string tag, CancellationToken token = default);

        Task ComplatedAsync(string tag, CancellationToken token = default);
    }
}
