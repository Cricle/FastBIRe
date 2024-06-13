namespace Diagnostics.Traces
{
    public sealed class Node<TKey, TValue>
    {
        internal Node(TKey key, TValue data)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Value = data;
            Key = key;
            Next = null;
            Previous = null;
        }

        public TValue Value;

        public TKey Key;

        public Node<TKey, TValue>? Next;

        public Node<TKey, TValue>? Previous;

        public override string ToString()
        {
            return $"Key:{Key} Data:{Value} Previous:{GetNodeSummary(Previous)} Next:{GetNodeSummary(Next)}";
        }

        private static string GetNodeSummary(Node<TKey, TValue>? node)
        {
            return node != null ? "Set" : "Null";
        }
    }

}