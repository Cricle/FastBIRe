using FastBIRe.Cdc.Checkpoints;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public interface IGetCdcListenerOptions
    {
        IReadOnlyList<string>? TableNames { get; }
    }
    public interface ICdcManager
    {
        Task<bool> IsDatabaseCdcEnableAsync(string databaseName, CancellationToken token = default);

        Task<bool> IsTableCdcEnableAsync(string databaseName, string tableName, CancellationToken token = default);

        Task<DbVariables> GetCdcVariablesAsync(CancellationToken token = default);

        Task<ICdcListener> GetCdcListenerAsync(IGetCdcListenerOptions options, CancellationToken token = default);

        Task<ICdcLogService> GetCdcLogServiceAsync(CancellationToken token = default);

        Task<ICheckPointManager> GetCdcCheckPointManagerAsync(CancellationToken token = default);
    }
}
