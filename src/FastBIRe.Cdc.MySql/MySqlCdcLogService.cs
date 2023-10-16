using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.MySql
{
    public class MySqlCdcLogService : CdcLogServiceBase
    {
        public MySqlCdcLogService(IScriptExecuter scriptExecuter)
        {
            ScriptExecuter = scriptExecuter;
        }

        public IScriptExecuter ScriptExecuter { get; }

        public override async Task<IList<ICdcLog>> GetAllAsync(CancellationToken token = default)
        {
            var logs = new List<ICdcLog>();
            await ScriptExecuter.ReadAsync("SHOW BINARY LOGS;", (s, e) =>
            {
                while (e.Reader.Read())
                {
                    logs.Add(ReadLog(e.Reader));
                }
                return Task.CompletedTask;
            }, token: token);
            return logs;
        }
        private MySqlCdcLog ReadLog(IDataReader reader)
        {
            var name = reader.GetString(0);
            var length = reader.GetInt64(1);
            var log = new MySqlCdcLog(name, (ulong)length);
            SetRecords(reader, log);
            return log;
        }
        public override async Task<ICdcLog?> GetLastAsync(CancellationToken token = default)
        {
            ICdcLog? log = null;
            await ScriptExecuter.ReadAsync("SHOW MASTER STATUS;", (s, e) =>
            {
                if (e.Reader.Read())
                {
                    log = ReadLog(e.Reader);
                }
                return Task.CompletedTask;
            }, token: token);
            return log;
        }
    }
}
