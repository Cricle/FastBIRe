using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public abstract class CdcLogServiceBase : ICdcLogService
    {
        public abstract Task<IList<ICdcLog>> GetAllAsync(CancellationToken token = default);
        public abstract Task<ICdcLog?> GetLastAsync(CancellationToken token = default);

        protected virtual void SetRecords(IDataReader reader, ICdcLog log)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                log[reader.GetName(i)] = reader[i];
            }
        }
    }
}
