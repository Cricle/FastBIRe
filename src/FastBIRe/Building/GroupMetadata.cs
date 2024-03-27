namespace FastBIRe.Building
{
    public class GroupMetadata : QueryMetadata, IEquatable<GroupMetadata>
    {
        public GroupMetadata(IQueryMetadata target)
            : this(new[] { target })
        {
        }
        public GroupMetadata(IList<IQueryMetadata> target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public IList<IQueryMetadata> Target { get; }

        public override IEnumerable<IQueryMetadata> GetChildren()
        {
            return Target;
        }

        public override string ToString()
        {
            return "group by (" + string.Join(",", Target) + ")";
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
            return Equals(obj as GroupMetadata);
        }

        public bool Equals(GroupMetadata? other)
        {
            if (other == null)
            {
                return false;
            }
            return other.Target.SequenceEqual(Target);
        }
    }
}
