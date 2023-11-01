using System.Text;

namespace FastBIRe.Building
{
    public class MultipleQueryMetadata : List<IQueryMetadata>, IEquatable<MultipleQueryMetadata>, IQueryMetadata
    {
        public MultipleQueryMetadata()
        {
        }

        public MultipleQueryMetadata(IEnumerable<IQueryMetadata> collection) : base(collection)
        {
        }

        public MultipleQueryMetadata(int capacity) : base(capacity)
        {
        }

        public bool Equals(MultipleQueryMetadata? other)
        {
            if (other == null || other.Count != Count)
            {
                return false;
            }
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].Equals(other[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            var hc = new HashCode();
            for (int i = 0; i < Count; i++)
            {
                hc.Add(this[i].GetHashCode());
            }
            return hc.ToHashCode();
        }

        public override string ToString()
        {
            if (Count == 0)
            {
                return string.Empty;
            }
            var s = string.Empty;
            for (int i = 0; i < Count; i++)
            {
                s += "(";
                s += this[i].ToString();
                s += ")";
            }
            return s;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as MultipleQueryMetadata);
        }

        public void ToString(StringBuilder builder)
        {
            builder.Append(ToString());
        }

        public IEnumerable<IQueryMetadata> GetChildren()
        {
            return this;
        }
    }
}
