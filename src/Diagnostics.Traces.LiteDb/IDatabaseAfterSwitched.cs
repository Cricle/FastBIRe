namespace Diagnostics.Traces.LiteDb
{
    public interface IDatabaseAfterSwitched
    {
        void AfterSwitched(LiteDatabaseCreatedResult result);
    }
}
