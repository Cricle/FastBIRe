namespace FastBIRe.Building
{
    public class FilterMetadata : MultipleQueryMetadata, IEquatable<FilterMetadata>
    {
        public FilterMetadata()
        {
        }
        public FilterMetadata(params IQueryMetadata[] collection)
            : base(collection)
        {
        }
        public FilterMetadata(IEnumerable<IQueryMetadata> collection) : base(collection)
        {
        }

        public FilterMetadata(int capacity) : base(capacity)
        {
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as FilterMetadata);
        }

        public string Combine(string @operator)
        {
            if (Count == 0)
            {
                return string.Empty;
            }
            if (Count == 1)
            {
                return this[0].ToString()!;
            }
            var s = string.Empty;
            for (int i = 0; i < Count; i++)
            {
                s += " (";
                s += this[i].ToString();
                s += ") ";
                if (i != Count - 1)
                {
                    s += @operator;
                }
            }
            return s;
        }
        public override string ToString()
        {
            return Combine("&&");
        }

        public bool Equals(FilterMetadata? other)
        {
            return base.Equals(other);
        }
    }
}
