﻿using System.Text;

namespace FastBIRe.Store
{
    public static class DataStoreStringExtensions
    {
        public static string? GetString(this IDataStore dataStore, string key, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var stream = dataStore.Get(key);
            if (stream != null)
            {
                using (stream)
                using (var reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
            return null;
        }
        public static async Task<string?> GetStringAsync(this IDataStore dataStore,string key,Encoding? encoding=null,CancellationToken token = default)
        {
            encoding ??= Encoding.UTF8;
            var stream = await dataStore.GetAsync(key,token);
            if (stream != null)
            {
                using (stream)
                using (var reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
            return null;
        }
        public static async Task SetStringAsync(this IDataStore dataStore, string key, string value, Encoding? encoding = null, CancellationToken token = default)
        {
            encoding ??= Encoding.UTF8;
            var buffer = encoding.GetBytes(value);
            using (var mem = new MemoryStream(buffer))
            {
                await dataStore.SetAsync(key, mem, token);
            }
        }
        public static void SetString(this IDataStore dataStore, string key, string value, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var buffer = encoding.GetBytes(value);
            using (var mem = new MemoryStream(buffer))
            {
                dataStore.Set(key, mem);
            }
        }
    }
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
