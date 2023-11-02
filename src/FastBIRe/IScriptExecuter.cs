using DatabaseSchemaReader;
using System.Data.Common;

namespace FastBIRe
{
    public static class DbScriptExecuterGetExtensions
    {
        public static DatabaseReader CreateReader(this IDbScriptExecuter dbScriptExecuter)
        {
            return new DatabaseReader(dbScriptExecuter.Connection) { Owner = dbScriptExecuter.Connection.Database };
        }
    }
    public delegate Task ReadDataHandler(IScriptExecuter executer, ReadingDataArgs args);
    public delegate Task<TResult> ReadDataResultHandler<TResult>(IScriptExecuter executer, ReadingDataArgs args);
    public interface IDbScriptExecuter : IScriptExecuter
    {
        DbConnection Connection { get; }
    }
    public interface IScriptExecuter : IDisposable
    {
        Task<int> ExecuteAsync(string script, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default);

        Task<int> ExecuteBatchAsync(IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null, CancellationToken token = default);

        Task ReadAsync(string script, ReadDataHandler handler, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default);

        Task<TResult> ReadResultAsync<TResult>(string script, ReadDataResultHandler<TResult> handler, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default);
    }
}
