namespace Diagnostics.Traces.Stores
{
    public interface IUndefinedDatabaseSelector<TResult>
        where TResult:IDatabaseCreatedResult
    {
        void UsingDatabaseResult(TraceTypes type, Action<TResult> @using);

        void ReportInserted(TraceTypes type, int count);
    }
}
