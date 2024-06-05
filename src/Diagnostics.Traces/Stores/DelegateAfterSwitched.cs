namespace Diagnostics.Traces.Stores
{
    public class DelegateAfterSwitched<TResult> : IUndefinedDatabaseAfterSwitched<TResult>
    {
        public DelegateAfterSwitched(Action<TResult> action)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public Action<TResult> Action { get; }

        public void AfterSwitched(TResult result)
        {
            Action(result);
        }
    }
}
