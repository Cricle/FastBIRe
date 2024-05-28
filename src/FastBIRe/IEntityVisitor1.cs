namespace FastBIRe
{
    public interface IEntityVisitor<T> : IEntityVisitor
    {
        bool TryGetValue(T instance, string propertyName, out object? value);

        bool TryGetValue<TValue>(T instance, string propertyName, out TValue? value);
    }
}
