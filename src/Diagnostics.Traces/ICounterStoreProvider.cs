namespace Diagnostics.Traces
{
    public interface ICounterStoreProvider
    {
        Task InitializeAsync(string name, IEnumerable<CounterStoreColumn> columns);

        Task InsertAsync(string name, IEnumerable<double?> values);

        Task InsertManyAsync(string name, IEnumerable<IEnumerable<double?>> values);
    }

    public readonly struct CounterStoreColumn
    {
        public CounterStoreColumn(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
