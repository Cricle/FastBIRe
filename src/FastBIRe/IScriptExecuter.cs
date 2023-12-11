namespace FastBIRe
{
    public delegate Task ReadDataHandler(IScriptExecuter executer, ReadingDataArgs args);
    public delegate void ReadDataHandlerSync(IScriptExecuter executer, ReadingDataArgs args);
    public delegate Task<TResult> ReadDataResultHandler<TResult>(IScriptExecuter executer, ReadingDataArgs args);
    public interface IScriptExecuter : IDisposable
    {
        Task<int> ExecuteAsync(string script, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default);

        Task<int> ExecuteBatchAsync(IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null, CancellationToken token = default);

        Task ReadAsync(string script, ReadDataHandler handler, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default);

        Task<TResult> ReadResultAsync<TResult>(string script, ReadDataResultHandler<TResult> handler, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default);
    }
    public static class ScriptExecuterEventExtensions
    {
        public static void RegistScriptStated(this IScriptExecuter executer, EventHandler<ScriptExecuteEventArgs> handler)
        {
            if (executer is DefaultScriptExecuter scriptExecuter)
            {
                scriptExecuter.ScriptStated += handler;
                return;
            }
            throw new InvalidCastException($"Can't cast {executer.GetType()} to {typeof(DefaultScriptExecuter)}");
        }
        public static void UnRegistScriptStated(this IScriptExecuter executer, EventHandler<ScriptExecuteEventArgs> handler)
        {
            if (executer is DefaultScriptExecuter scriptExecuter)
            {
                scriptExecuter.ScriptStated -= handler;
                return;
            }
            throw new InvalidCastException($"Can't cast {executer.GetType()} to {typeof(DefaultScriptExecuter)}");
        }
    }
}
