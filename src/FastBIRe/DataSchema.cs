namespace FastBIRe
{
    public record class DataSchema
    {
        public DataSchema(IReadOnlyList<string> names, IReadOnlyList<Type> types)
        {
            Names = names;
            Types = types;
            TypeCodes = types.Select(x => Convert.GetTypeCode(x)).ToArray();
        }

        public IReadOnlyList<string> Names { get; }

        public IReadOnlyList<Type> Types { get; }

        public IReadOnlyList<TypeCode> TypeCodes { get; }
    }
}
