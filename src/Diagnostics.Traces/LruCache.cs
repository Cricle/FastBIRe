namespace Diagnostics.Traces
{
    public class LruCache<TKey, TValue> : IDisposable
        where TKey : notnull
    {
        private const int DefaultCacheSize = 1000;
        private const int MinimumCacheSize = 2;

        protected readonly Dictionary<TKey, Node<TKey, TValue>> data;
        private Node<TKey, TValue>? head;
        private Node<TKey, TValue>? tail;
        private readonly int cacheSize;
        protected readonly object locker;

        public LruCache(int capacity = DefaultCacheSize)
        {
            if (capacity < MinimumCacheSize)
            {
                throw new ArgumentException("Cache size must be at least 2", nameof(capacity));
            }

            cacheSize = capacity;
            data = new Dictionary<TKey, Node<TKey, TValue>>();
            head = null;
            tail = null;
            locker = new object();
        }

        public int Count
        {
            get
            {
                lock (locker)
                {
                    return data.Count;
                }
            }
        }

        public int Capacity => cacheSize;

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get => TryGetValue(key, out var val) ? val! : throw new KeyNotFoundException(key.ToString());
            set => AddOrUpdate(key, value);
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> dataFun)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            lock (locker)
            {
                var d = dataFun(key);
                if (data.TryGetValue(key, out var node))
                {
                    MoveNodeUp(node);
                    node.Value = d;
                }
                else
                {
                    AddItem(key, d);
                }
                return d;
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> dataFun)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (dataFun == null)
            {
                throw new ArgumentNullException(nameof(dataFun));
            }

            lock (locker)
            {
                if (data.TryGetValue(key, out var node))
                {
                    MoveNodeUp(node);
                    return node.Value;
                }
                var d = dataFun(key);
                AddItem(key, d);
                return d;
            }
        }

        public TValue AddOrUpdate(TKey key, TValue data)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            lock (locker)
            {
                if (this.data.TryGetValue(key, out var node))
                {
                    MoveNodeUp(node);
                    node.Value = data;
                }
                else
                {
                    AddItem(key, data);
                }
                return data;
            }
        }

        public bool TryPeek(TKey key, out TValue? data)
        {
            lock (locker)
            {
                if (this.data.TryGetValue(key, out var node))
                {
                    data = node.Value;
                    return true;
                }
                else
                {
                    data = default;
                    return false;
                }
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                foreach (var item in data)
                {
                    DisposeValue(item.Value.Value);
                }
                data.Clear();
                head = null;
                tail = null;
            }
        }

        public bool TryGetValue(TKey key, out TValue? data)
        {
            lock (locker)
            {
                if (this.data.TryGetValue(key, out var node))
                {
                    data = node.Value;
                    MoveNodeUp(node);
                    return true;
                }
                else
                {
                    data = default;
                    return false;
                }
            }
        }

        public bool Remove(TKey key)
        {
            lock (locker)
            {
                if (data.TryGetValue(key, out var node))
                {
                    RemoveNodeFromList(node);
                    DisposeValue(node.Value);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        protected virtual void DisposeValue(TValue value)
        {

        }

        public bool ContainsKey(TKey key)
        {
            lock (locker)
            {
                return data.ContainsKey(key);
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (locker)
            {
                if (!data.ContainsKey(key))
                {
                    AddItem(key, value);
                }
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (locker)
            {
                Add(item.Key, item.Value);
            }
        }

        protected void RemoveNodeFromList(Node<TKey, TValue> node)
        {
            data.Remove(node.Key);
            if (node.Previous != null)
            {
                node.Previous.Next = node.Next;
            }
            if (node.Next != null)
            {
                node.Next.Previous = node.Previous;
            }
            if (node == head)
            {
                head = node.Next;
            }
            if (node == tail)
            {
                tail = node.Previous;
            }
            node.Previous = null;
            node.Next = null;
        }

        private void MoveNodeUp(Node<TKey, TValue> node)
        {
            if (node == head)
            {
                return;
            }

            if (node.Previous != null)
            {
                if (node == tail)
                {
                    tail = node.Previous;
                }
                node.Previous.Next = node.Next;
            }
            if (node.Next != null)
            {
                node.Next.Previous = node.Previous;
            }
            node.Next = head;
            if (head != null)
            {
                head.Previous = node;
            }
            node.Previous = null;
            head = node;
        }

        private void AddItem(TKey key, TValue value)
        {
            lock (locker)
            {
                var node = new Node<TKey, TValue>(key, value);
                data[key] = node;

                if (head == null)
                {
                    head = node;
                    tail = node;
                }
                else
                {
                    head.Previous = node;
                    node.Next = head;
                    head = node;

                    if (Count > cacheSize)
                        RemoveNodeFromList(tail!);
                }
            }
        }

        public void Dispose()
        {
            Clear();
            OnDisposed();
        }

        protected virtual void OnDisposed()
        {

        }
    }

}