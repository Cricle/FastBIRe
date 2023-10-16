using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public class CdcLog : Dictionary<string, object>, ICdcLog
    {
        public CdcLog(string name, ulong? length)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            Name = name;
            Length = length;
        }

        public string Name { get; }

        public ulong? Length { get; }
    }
    public interface ICdcLog : IDictionary<string, object>
    {
        string Name { get; }

        ulong? Length { get; }
    }
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
    public interface ICdcLogService
    {
        Task<IList<ICdcLog>> GetAllAsync(CancellationToken token = default);

        Task<ICdcLog?> GetLastAsync(CancellationToken token = default);
    }
}
