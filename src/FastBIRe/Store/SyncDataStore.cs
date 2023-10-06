namespace FastBIRe.Store
{
    public abstract class SyncDataStore : IDataStore
    {
        protected SyncDataStore(string nameSpace)
        {
            NameSpace = nameSpace;
        }

        public string NameSpace { get; }

        public abstract void Clear();

        public virtual Task ClearAsync(CancellationToken token = default)
        {
            Clear();
            return Task.CompletedTask;
        }

        public abstract bool Exists(string key);

        public virtual Task<bool> ExistsAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Exists(key));
        }

        public abstract Stream? Get(string key);

        public virtual Task<Stream?> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        public abstract bool Remove(string key);

        public virtual Task<bool> RemoveAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Remove(key));
        }

        public abstract void Set(string key, Stream value);

        public virtual Task SetAsync(string key, Stream value, CancellationToken token = default)
        {
            Set(key, value);
            return Task.CompletedTask;
        }
    }
}
