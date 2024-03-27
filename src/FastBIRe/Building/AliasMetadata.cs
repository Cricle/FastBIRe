namespace FastBIRe.Building
{

    public class AliasMetadata : QueryMetadata, IEquatable<AliasMetadata>
    {
        public AliasMetadata(IQueryMetadata target, string alias)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
        }

        public IQueryMetadata Target { get; }

        public string Alias { get; }

        public override IEnumerable<IQueryMetadata> GetChildren()
        {
            yield return Target;
        }
        public bool Equals(AliasMetadata? other)
        {
            if (other == null)
            {
                return false;
            }
            return other.Target.Equals(Target) &&
                other.Alias.Equals(Alias);
        }

        public override string ToString()
        {
            return $"{Target} as {Alias}";
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Target, Alias);
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as AliasMetadata);
        }
    }
}
