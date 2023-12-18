using System.Data.Common;
using System.Diagnostics;

namespace FastBIRe
{
    public delegate Task ReadDataHandler(IScriptExecuter executer, ReadingDataArgs args);
    public delegate void ReadDataHandlerSync(IScriptExecuter executer, ReadingDataArgs args);
    public delegate Task<TResult> ReadDataResultHandler<TResult>(IScriptExecuter executer, ReadingDataArgs args);
    public delegate TResult ReadDataResultHandlerSync<TResult>(IScriptExecuter executer, ReadingDataArgs args);
    public interface IScriptExecuter : IDisposable
    {
        Task<int> ExecuteAsync(string script, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default);

        int Execute(string script, IEnumerable<KeyValuePair<string, object?>>? args = null);

        Task<int> ExecuteBatchAsync(IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null, CancellationToken token = default);

        int ExecuteBatch(IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null);

        Task ReadAsync(string script, ReadDataHandler handler, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default);

        void Read(string script, ReadDataHandlerSync handler, IEnumerable<KeyValuePair<string, object?>>? args = null);

        Task<IScriptReadResult> ReadAsync(string script, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default);

        IScriptReadResult Read(string script, IEnumerable<KeyValuePair<string, object?>>? args = null);

        Task<TResult> ReadResultAsync<TResult>(string script, ReadDataResultHandler<TResult> handler, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default);
        
        TResult ReadResult<TResult>(string script, ReadDataResultHandlerSync<TResult> handler, IEnumerable<KeyValuePair<string, object?>>? args = null);
    }
    public interface IScriptReadResult : IDisposable
    {
        IScriptExecuter Executer { get; }

        ReadingDataArgs Args { get; }

        T? Read<T>();
    }
    public readonly struct DefaultScriptReadResult : IScriptReadResult
    {
        public DefaultScriptReadResult(IScriptExecuter executer, ReadingDataArgs args, DbCommand command, Action endRead)
        {
            Executer = executer;
            Args = args;
            EndRead = endRead;
            this.command = command;
        }
        private readonly DbCommand command;
        private readonly Action EndRead;

        public IScriptExecuter Executer { get; }

        public ReadingDataArgs Args { get; }

        public void Dispose()
        {
            command.Dispose();
            Args.Reader.Dispose();
            EndRead();
        }

        public T? Read<T>()
        {
            return RecordToObjectManager<T>.To(Args.Reader);
        }
    }
}
