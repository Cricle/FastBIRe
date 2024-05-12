namespace Diagnostics.Traces.Stores
{
    public interface IUndefinedDatabaseAfterSwitched<TResult>
    {
        void AfterSwitched(TResult result);
    }
}
