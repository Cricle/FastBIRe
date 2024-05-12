using Diagnostics.Traces.Stores;

namespace Diagnostics.Traces.LiteDb
{
    public interface IDatabaseAfterSwitched: IUndefinedDatabaseAfterSwitched<LiteDatabaseCreatedResult>
    {
    }
}
