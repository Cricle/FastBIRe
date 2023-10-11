using System.Data.Common;
using System.Diagnostics;

namespace FastBIRe
{
    public interface IDbStackTraceScriptExecuter: IStackTraceScriptExecuter
    {
        DbConnection Connection { get; }
    }
    public interface IStackTraceScriptExecuter
    {
        Task<int> ExecuteAsync(string script, StackTrace? stackTrace, CancellationToken token = default);

        Task<int> ExecuteBatchAsync(IEnumerable<string> scripts, StackTrace? stackTrace, CancellationToken token = default);
    }
}
