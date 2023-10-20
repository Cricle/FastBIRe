namespace FastBIRe
{
    public record class DataSchema
    {
        public DataSchema(IReadOnlyList<string> names, IReadOnlyList<Type> types)
        {
            Names = names;
            Types = types;
        }

        public IReadOnlyList<string> Names { get; }

        public IReadOnlyList<Type> Types { get; }
    }
}
