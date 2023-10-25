using FastBIRe.Cdc.Checkpoints;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public interface ICdcManager
    {
        CdcOperators SupportCdcOperators { get; }

        Task<bool?> TryEnableDatabaseCdcAsync(string databaseName, CancellationToken token = default);

        Task<bool?> TryEnableTableCdcAsync(string databaseName,string tableName, CancellationToken token = default);

        Task<bool?> TryDisableDatabaseCdcAsync(string databaseName, CancellationToken token = default);

        Task<bool?> TryDisableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default);

        Task<bool> IsDatabaseSupportAsync(CancellationToken token = default);

        Task<bool> IsDatabaseCdcEnableAsync(string databaseName, CancellationToken token = default);

        Task<bool> IsTableCdcEnableAsync(string databaseName, string tableName, CancellationToken token = default);

        Task<DbVariables> GetCdcVariablesAsync(CancellationToken token = default);

        Task<ICdcListener> GetCdcListenerAsync(IGetCdcListenerOptions options, CancellationToken token = default);

        Task<ICdcLogService> GetCdcLogServiceAsync(CancellationToken token = default);

        Task<ICheckPointManager> GetCdcCheckPointManagerAsync(CancellationToken token = default);
    }
}
