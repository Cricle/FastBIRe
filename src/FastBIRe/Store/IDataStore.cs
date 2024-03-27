namespace FastBIRe.Store
{
    public interface IDataStore
    {
        string NameSpace { get; }

        Stream? Get(string key);

        Task<Stream?> GetAsync(string key, CancellationToken token = default);

        void Set(string key, Stream value);

        Task SetAsync(string key, Stream value, CancellationToken token = default);

        bool Remove(string key);

        Task<bool> RemoveAsync(string key, CancellationToken token = default);

        bool Exists(string key);

        Task<bool> ExistsAsync(string key, CancellationToken token = default);

        void Clear();

        Task ClearAsync(CancellationToken token = default);
    }
}
