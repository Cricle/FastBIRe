namespace Diagnostics.Traces.Stores
{
    public interface IUndefinedResultInitializer<TResult>
    {
        void InitializeResult(TResult result);
    }
}
