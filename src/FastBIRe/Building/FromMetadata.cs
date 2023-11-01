namespace FastBIRe.Building
{
    public class FromMetadata : QueryMetadata
    {
        public FromMetadata(IQueryMetadata from)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
        }

        public IQueryMetadata From { get; }
        public override IEnumerable<IQueryMetadata> GetChildren()
        {
            return Enumerable.Empty<IQueryMetadata>();
        }
        public override int GetHashCode()
        {
            return From.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is IQueryMetadata metadata)
            {
                return metadata.Equals(this);
            }
            return false;
        }
        public override string? ToString()
        {
            return From.ToString();
        }
    }
}
