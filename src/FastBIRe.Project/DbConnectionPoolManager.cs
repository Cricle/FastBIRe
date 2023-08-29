using System.Collections.Concurrent;

namespace FastBIRe.Project
{
    public class DbConnectionPoolManager<TKey> : IDisposable
#if NET6_0_OR_GREATER
        where TKey : notnull
#endif
    {
        private readonly ConcurrentDictionary<TKey, DbConnectionPool> pools = new ConcurrentDictionary<TKey, DbConnectionPool>();

        public IReadOnlyDictionary<TKey, DbConnectionPool> Pools => pools;

        public bool Add(TKey key, DbConnectionPool pool)
        {
            return pools.TryAdd(key, pool);
        }
        public DbConnectionPool GetOrAdd(TKey key, Func<TKey, DbConnectionPool> creator)
        {
            return pools.GetOrAdd(key, creator);
        }

        public bool Remove(TKey key)
        {
            var r = pools.TryRemove(key, out var p);
            p?.Dispose();
            return r;
        }

        public void Clean()
        {
            foreach (var pool in pools.Values)
            {
                pool.Dispose();
            }
            pools.Clear();
        }

        public void Dispose()
        {
            Clean();
        }
    }
}
