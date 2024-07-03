namespace Diagnostics.Traces
{
    public interface IBytesStoreManager:IEnumerable<IBytesStore>,IDisposable
    {
        int Count { get; }

        IBytesStore GetOrAdd(string name);

        bool Remove(string name);

        bool Exists(string name);
    }
}
