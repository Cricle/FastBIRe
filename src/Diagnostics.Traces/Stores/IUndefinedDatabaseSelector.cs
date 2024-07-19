namespace Diagnostics.Traces.Stores
{
    public class DelegateDatabaseSelectorFactory<TResult, TInput> : IUndefinedDatabaseSelectorFactory<TResult, TInput>
        where TResult : IDatabaseCreatedResult
    {
        public DelegateDatabaseSelectorFactory(Func<TInput, IUndefinedDatabaseSelector<TResult>> creator)
        {
            Creator = creator;
        }

        public Func<TInput, IUndefinedDatabaseSelector<TResult>> Creator { get; }

        public IUndefinedDatabaseSelector<TResult> Create(TInput input)
        {
            return Creator(input);
        }
    }
    public interface IUndefinedDatabaseSelectorFactory<TResult,TInput>
        where TResult : IDatabaseCreatedResult
    {
        IUndefinedDatabaseSelector<TResult> Create(TInput input);
    }
    public interface IUndefinedDatabaseSelector<TResult> : IDisposable
        where TResult:IDatabaseCreatedResult
    {
        IList<IUndefinedDatabaseAfterSwitched<TResult>> AfterSwitcheds { get; }

        IList<IUndefinedResultInitializer<TResult>> Initializers { get; }

        void UsingDatabaseResult(Action<TResult> @using);

        void UnsafeUsingDatabaseResult(Action<TResult> @using);

        TReturn UsingDatabaseResult<TReturn>(Func<TResult, TReturn> @using);

        TReturn UnsafeUsingDatabaseResult<TReturn>(Func<TResult, TReturn> @using);

        void UsingDatabaseResult<TState>(TState state, Action<TResult, TState> @using);
        
        void UnsafeUsingDatabaseResult<TState>(TState state, Action<TResult, TState> @using);

        TReturn UsingDatabaseResult<TState, TReturn>(TState state, Func<TResult, TState, TReturn> @using);

        TReturn UnsafeUsingDatabaseResult<TState,TReturn>(TState state, Func<TResult, TState, TReturn> @using);

        void ReportInserted(int count);

        void UnsafeReportInserted(int count);

        bool Flush();
    }
}
