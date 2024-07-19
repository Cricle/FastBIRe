using Diagnostics.Traces.Stores;

namespace Diagnostics.Traces.Parquet
{
    public class ParquetBytesManager : BytesStoreManagerBase
    {
        public ParquetBytesManager(IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult> databaseSelector)
        {
            DatabaseSelector = databaseSelector ?? throw new ArgumentNullException(nameof(databaseSelector));
        }

        public IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult> DatabaseSelector { get; }

        protected override IBytesStore CreateStringStore(string name)
        {
            return new ParquetStringStore(DatabaseSelector, name);
        }
    }
}
