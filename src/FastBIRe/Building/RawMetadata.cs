namespace FastBIRe.Building
{
    public class RawMetadata : QueryMetadata
    {
        public RawMetadata(string? raw)
        {
            Raw = raw;
        }

        public string? Raw { get; }

        public override string? ToString()
        {
            return Raw;
        }
        public override int GetHashCode()
        {
            return Raw?.GetHashCode() ?? 0;
        }
    }
}
