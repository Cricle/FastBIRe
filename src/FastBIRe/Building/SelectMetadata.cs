namespace FastBIRe.Building
{
    public class SelectMetadata : QueryMetadata, IEquatable<SelectMetadata>
    {
        public SelectMetadata(IQueryMetadata target)
            : this(new[] { target })
        {
        }
        public SelectMetadata(params IQueryMetadata[] target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
        public SelectMetadata(IList<IQueryMetadata> target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public IList<IQueryMetadata> Target { get; }

        public override IEnumerable<IQueryMetadata> GetChildren()
        {
            return Target;
        }

        public override string? ToString()
        {
            return "select (" + string.Join(",", Target) + ")";
        }

        public override int GetHashCode()
        {
            var hc = new HashCode();
            for (int i = 0; i < Target.Count; i++)
            {
                hc.Add(Target[i]);
            }
            return hc.ToHashCode();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as SelectMetadata);
        }

        public bool Equals(SelectMetadata? other)
        {
            if (other == null)
            {
                return false;
            }
            return other.Target.SequenceEqual(Target);
        }
    }
}
