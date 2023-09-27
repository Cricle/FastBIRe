using System.Data.Common;

namespace FastBIRe
{
    public interface IScriptExecuter
    {
        Task<int> ExecuteAsync(string script, CancellationToken token);

        Task<int> ExecuteAsync(IEnumerable<string> scripts, CancellationToken token);
    }
}
