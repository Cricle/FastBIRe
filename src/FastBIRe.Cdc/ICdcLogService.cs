using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public interface ICdcLogService
    {
        Task<IList<ICdcLog>> GetAllAsync(CancellationToken token = default);

        Task<ICdcLog?> GetLastAsync(CancellationToken token = default);
    }
}
