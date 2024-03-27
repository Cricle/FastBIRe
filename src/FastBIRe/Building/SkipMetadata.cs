namespace FastBIRe.Building
{
    public class SkipMetadata : ValueMetadata<int>
    {
        public SkipMetadata(int value) : base(value)
        {
        }

        public SkipMetadata(int value, bool quto) : base(value, quto)
        {
        }
        public override string ToString()
        {
            return "skip " + base.ToString();
        }
    }
}
