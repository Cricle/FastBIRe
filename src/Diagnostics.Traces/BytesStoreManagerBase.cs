using System.Collections;
using System.Collections.Concurrent;

namespace Diagnostics.Traces
{
    public abstract class BytesStoreManagerBase : IBytesStoreManager
    {
        private readonly ConcurrentDictionary<string, IBytesStore> bytesStores;

        protected BytesStoreManagerBase()
        {
            bytesStores = new ConcurrentDictionary<string, IBytesStore>(StringComparer.OrdinalIgnoreCase);
        }

        public int Count => bytesStores.Count;

        public bool Exists(string name)
        {
            return bytesStores.ContainsKey(name);
        }

        public IEnumerator<IBytesStore> GetEnumerator()
        {
            return bytesStores.Values.GetEnumerator();
        }

        public IBytesStore GetOrAdd(string name)
        {
            return bytesStores.GetOrAdd(name, CreateStringStore);
        }

        protected abstract IBytesStore CreateStringStore(string name);

        public bool Remove(string name)
        {
            if (bytesStores.TryGetValue(name,out var val))
            {
                val.Dispose();
                return true;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            foreach (var store in bytesStores.Values)
            {
                store.Dispose();
            }
            bytesStores.Clear();
            OnDisposed();
        }

        protected virtual void OnDisposed()
        {

        }
    }
}
