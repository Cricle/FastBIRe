using Diagnostics.Traces.Stores;

namespace Diagnostics.Traces.Mini
{
    public class MiniBytesStoreManager : BytesStoreManagerBase
    {
        public MiniBytesStoreManager(IUndefinedDatabaseSelector<MiniDatabaseCreatedResult> databaseSelector)
        {
            DatabaseSelector = databaseSelector ?? throw new ArgumentNullException(nameof(databaseSelector));
        }

        public IUndefinedDatabaseSelector<MiniDatabaseCreatedResult> DatabaseSelector { get; }

        protected override IBytesStore CreateStringStore(string name)
        {
            return new MiniStringStore(DatabaseSelector, name);
        }
    }
}
