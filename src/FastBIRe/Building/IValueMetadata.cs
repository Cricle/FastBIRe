namespace FastBIRe.Building
{
    public interface IValueMetadata : IQueryMetadata
    {
        object? Value { get; }

        bool Quto { get; }
    }
    public interface IValueMetadata<T> : IValueMetadata
    {
        new T Value { get; }
    }
}
