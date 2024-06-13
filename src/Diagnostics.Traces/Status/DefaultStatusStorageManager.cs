using System.Collections;

namespace Diagnostics.Traces.Status
{
    public class DefaultStatusStorageManager : IStatusStorageManager
    {
        private readonly object locker = new object();
        private readonly Dictionary<string,IStatusStorage> storages = new Dictionary<string, IStatusStorage>();

        public int Count
        {
            get
            {
                lock (locker)
                {
                    return storages.Count;
                }
            }
        }

        bool ICollection<IStatusStorage>.IsReadOnly => false;

        public void Add(IStatusStorage item)
        {
            lock (locker)
            {
                storages[item.Name] = item;
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                storages.Clear();
            }
        }

        public bool Contains(IStatusStorage item)
        {
            lock (locker)
            {
                return storages.ContainsKey(item.Name);
            }
        }

        public void CopyTo(IStatusStorage[] array, int arrayIndex)
        {
            lock (locker)
            {
                storages.Values.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<IStatusStorage> GetEnumerator()
        {
            return storages.Values.GetEnumerator();
        }

        public bool Remove(IStatusStorage item)
        {
            lock (locker)
            {
                return storages.Remove(item.Name);
            }
        }

        public bool TryGetValue(string name, out IStatusStorage storage)
        {
            lock (locker)
            {
                return storages.TryGetValue(name, out storage);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
