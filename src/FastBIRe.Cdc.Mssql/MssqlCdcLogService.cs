using System.Data;

namespace FastBIRe.Cdc.Mssql
{
    public class MssqlCdcLogService : CdcLogServiceBase
    {
        public MssqlCdcLogService(IDbScriptExecuter scriptExecuter)
        {
            ScriptExecuter = scriptExecuter;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public override async Task<IList<ICdcLog>> GetAllAsync(CancellationToken token = default)
        {
            var var = new List<ICdcLog>();
            await ScriptExecuter.ReadAsync($"SELECT * FROM sys.master_files AS [b] WHERE [b].type_desc = 'LOG' AND EXISTS (SELECT 1 from sys.databases AS [a] WHERE [a].name='{ScriptExecuter.Connection.Database}' and [a].database_id=[b].database_id);", (s, r) =>
            {
                while (r.Reader.Read())
                {
                    var.Add(CreateLog(r.Reader));
                }
                return Task.CompletedTask;
            }, token: token);
            return var;
        }

        protected virtual ICdcLog CreateLog(IDataReader reader)
        {
            var name = reader.GetString(reader.GetOrdinal("physical_name"));
            var size = reader.GetInt64(reader.GetOrdinal("size"));
            var log = new CdcLog(name, (ulong)size);
            SetRecords(reader, log);
            return log;
        }

        public override async Task<ICdcLog?> GetLastAsync(CancellationToken token = default)
        {
            ICdcLog? log = null;
            await ScriptExecuter.ReadAsync($"SELECT TOP 1 * FROM sys.master_files AS [b] WHERE [b].type_desc = 'LOG' AND EXISTS (SELECT 1 from sys.databases AS [a] WHERE [a].name='{ScriptExecuter.Connection.Database}' and [a].database_id=[b].database_id) ORDER BY [b].file_id DESC;", (s, r) =>
            {
                if (r.Reader.Read())
                {
                    log = CreateLog(r.Reader);
                }
                return Task.CompletedTask;
            }, token: token);
            return log;
        }
    }
}
