namespace FastBIRe.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class IdAttribute : Attribute
    {
        public IdAttribute(string id)
        {
            Id = id;
        }

        public IdAttribute(int id)
        {
            Id = id;
        }

        public IdAttribute(double id)
        {
            Id = id;
        }

        public object Id { get; }
    }
}
