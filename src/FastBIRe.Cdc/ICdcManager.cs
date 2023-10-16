using System.Threading.Tasks;
using System.Threading;

namespace FastBIRe.Cdc
{
    public interface ICdcManager
    {
        Task<bool> IsDatabaseCdcEnableAsync(string databaseName, CancellationToken token = default);
        Task<bool> IsTableCdcEnableAsync(string databaseName, string tableName, CancellationToken token = default);
        Task<DbVariables> GetCdcVariablesAsync(CancellationToken token = default);

        Task<ICdcListener> GetCdcListenerAsync(CancellationToken token = default);

        Task<ICdcLogService > GetCdcLogServiceAsync(CancellationToken token = default);
    }
}
