namespace FastBIRe.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class DecimalAttribute : Attribute
    {
        public DecimalAttribute(int precision = 18, int scale = 2)
        {
            Precision = precision;
            Scale = scale;
        }

        public int Precision { get; }

        public int Scale { get; }
    }
}
