using Diagnostics.Traces.Stores;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBBytesManager : BytesStoreManagerBase
    {
        public DuckDBBytesManager(IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> databaseSelector)
        {
            DatabaseSelector = databaseSelector ?? throw new ArgumentNullException(nameof(databaseSelector));
        }

        public IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> DatabaseSelector { get; }

        protected override IBytesStore CreateStringStore(string name)
        {
            return new DuckDBStringStore(DatabaseSelector, name);
        }
    }
}
