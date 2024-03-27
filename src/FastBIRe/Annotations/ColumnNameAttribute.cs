namespace FastBIRe.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false,Inherited = false)]
    public sealed class ColumnNameAttribute:Attribute
    {
        public ColumnNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
