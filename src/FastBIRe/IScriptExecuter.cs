using System.Data.Common;

namespace FastBIRe
{
    public delegate Task ReadDataHandler(IScriptExecuter executer, ReadingDataArgs args);
    public interface IDbScriptExecuter:IScriptExecuter
    {
        DbConnection Connection { get; }
    }
    public interface IScriptExecuter : IDisposable
    {
        Task<int> ExecuteAsync(string script, CancellationToken token = default);

        Task<int> ExecuteBatchAsync(IEnumerable<string> scripts, CancellationToken token = default);

        Task ReadAsync(string script, ReadDataHandler handler, CancellationToken token = default);
    }
}
