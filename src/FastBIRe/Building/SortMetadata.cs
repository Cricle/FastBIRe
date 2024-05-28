namespace FastBIRe.Building
{
    public class SortMetadata : QueryMetadata, IEquatable<SortMetadata>, IQueryMetadata
    {
        public SortMetadata(IQueryMetadata target, SortMode sortMode)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            SortMode = sortMode;
        }

        public SortMode SortMode { get; }

        public IQueryMetadata Target { get; }

        public override IEnumerable<IQueryMetadata> GetChildren()
        {
            yield return Target;
        }

        public override string ToString()
        {
            return $"order by {Target} {SortMode}";
        }

        public bool Equals(SortMetadata? other)
        {
            if (other == null)
            {
                return false;
            }
            return other.SortMode == SortMode &&
                other.Target.Equals(Target);
        }


        public override bool Equals(object? obj)
        {
            return Equals(obj as SortMetadata);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SortMode, Target);
        }
    }
}
