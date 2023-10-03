using System.Collections.Concurrent;

namespace FastBIRe.Store
{
    public class MemoryDataStore : IDataStore
    {
        public string NameSpace { get; }

        private readonly ConcurrentDictionary<string, Stream> datas = new ConcurrentDictionary<string, Stream>();

        public MemoryDataStore(string nameSpace)
        {
            NameSpace = nameSpace;
        }

        public void Clear()
        {
            datas.Clear();
        }

        public Task ClearAsync(CancellationToken token = default)
        {
            datas.Clear();
            return Task.CompletedTask;
        }

        public bool Exists(string key)
        {
            return datas.ContainsKey(key);
        }

        public Task<bool> ExistsAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Exists(key));
        }

        public Stream? Get(string key)
        {
            if (datas.TryGetValue(key, out var s))
            {
                var mem = new MemoryStream();
                s.CopyTo(mem);
            }
            return s;
        }

        public async Task<Stream?> GetAsync(string key, CancellationToken token = default)
        {
            if (datas.TryGetValue(key, out var s))
            {
                var mem = new MemoryStream();
                await s.CopyToAsync(mem, token);
            }
            return s;
        }

        public bool Remove(string key)
        {
            if (datas.TryRemove(key, out var s))
            {
                s.Dispose();
                return true;
            }
            return false;
        }

        public Task<bool> RemoveAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Remove(key));
        }

        public void Set(string key, Stream value)
        {
            datas.AddOrUpdate(key, k =>
            {
                var mem = new MemoryStream();
                value.CopyTo(mem);
                return mem;
            }, (k, old) =>
            {
                old.Dispose();
                var mem = new MemoryStream();
                value.CopyTo(mem);
                return mem;
            });
        }

        public Task SetAsync(string key, Stream value, CancellationToken token = default)
        {
            Set(key, value);
            return Task.CompletedTask;
        }
    }
}
