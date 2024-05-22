namespace Diagnostics.Traces.Stores
{
    public interface IUndefinedDatabaseSelector<TResult>
        where TResult:IDatabaseCreatedResult
    {
        IList<IUndefinedDatabaseAfterSwitched<TResult>> AfterSwitcheds { get; }

        IList<IUndefinedResultInitializer<TResult>> Initializers { get; }

        void UsingDatabaseResult(Action<TResult> @using);

        void UsingDatabaseResult<TState>(TState state, Action<TResult,TState> @using);

        void ReportInserted(int count);

        bool Flush();
    }
}
