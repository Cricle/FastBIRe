namespace Diagnostics.Traces
{
    public interface IBytesStore:IDisposable
    {
        string Name { get; }

        Task InsertAsync(BytesStoreValue value);

        void Insert(BytesStoreValue value);

        Task InsertManyAsync(IEnumerable<BytesStoreValue> strings);

        void InsertMany(IEnumerable<BytesStoreValue> strings);

        Task<int> CountAsync();

        int Count();
    }
}
