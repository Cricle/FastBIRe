namespace FastBIRe
{
    public interface IEntityVisitor
    {
        IReadOnlyCollection<string> PropertyNames { get; }

        bool ContainsProperty(string propertyName);

        bool TryGetValue(object instance, string propertyName, out object? value);

        bool TryGetValue<T>(object instance, string propertyName, out T? value);
    }
}
