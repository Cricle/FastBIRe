namespace Diagnostics.Traces.Stores
{
    public interface IUndefinedDatabaseSelector<TResult>
        where TResult:IDatabaseCreatedResult
    {
        void UsingDatabaseResult(TraceTypes type, Action<TResult> @using);

        void UsingDatabaseResult<TState>(TraceTypes type,TState state, Action<TResult,TState> @using);

        void ReportInserted(TraceTypes type, int count);
    }
}
