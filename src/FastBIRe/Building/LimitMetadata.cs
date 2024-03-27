namespace FastBIRe.Building
{
    public class LimitMetadata : ValueMetadata<int>
    {
        public LimitMetadata(int value) : base(value)
        {
        }

        public LimitMetadata(int value, bool quto) : base(value, quto)
        {
        }
        public override string ToString()
        {
            return "limit " + base.ToString();
        }
    }
}
