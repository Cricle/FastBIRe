namespace FastBIRe.Store
{
    public class FolderDataStore : IDataStore
    {
        public FolderDataStore(string folder, string nameSpace)
        {
            Folder = folder ?? throw new ArgumentNullException(nameof(folder));
            NameSpace = nameSpace ?? throw new ArgumentNullException(nameof(nameSpace));
        }

        public string NameSpace { get; }

        public string Folder { get; }

        public void Clear()
        {
            foreach (var item in Directory.EnumerateFiles(Folder))
            {
                File.Delete(item);
            }
        }

        public Task ClearAsync(CancellationToken token = default)
        {
            Clear();
            return Task.CompletedTask;
        }

        public bool Exists(string key)
        {
            var path = Path.Combine(Folder, key);
            return File.Exists(path);
        }

        public Task<bool> ExistsAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Exists(key));
        }

        public Stream? Get(string key)
        {
            if (Exists(key))
            {
                return File.OpenRead(Path.Combine(Folder, key));
            }
            return null;
        }

        public Task<Stream?> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        public bool Remove(string key)
        {
            if (Exists(key))
            {
                File.Delete(Path.Combine(Folder, key));
                return true;
            }
            return false;
        }

        public Task<bool> RemoveAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Exists(key));
        }

        public void Set(string key, Stream value)
        {
            var path = Path.Combine(Folder, key);
            using (var fs = File.Open(path, FileMode.Create))
            {
                value.CopyTo(fs);
            }
        }

        public async Task SetAsync(string key, Stream value, CancellationToken token = default)
        {
            var path = Path.Combine(Folder, key);
            using (var fs = File.Open(path, FileMode.Create))
            {
                await value.CopyToAsync(fs, 81920, token);
            }
        }
    }
}
