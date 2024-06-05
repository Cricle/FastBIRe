namespace Diagnostics.Traces.Stores
{
    public interface IUndefinedResultInitializer<TResult>
    {
        void InitializeResult(TResult result);
    }
    public class DelegateResultInitializer<TResult> : IUndefinedResultInitializer<TResult>
    {
        public DelegateResultInitializer(Action<TResult> action)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public Action<TResult> Action { get; }

        public void InitializeResult(TResult result)
        {
            Action(result);
        }
    }
}
