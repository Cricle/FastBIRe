namespace FastBIRe.Cdc.NpgSql
{
    public class PgSqlCdcLogService : CdcLogServiceBase
    {
        public PgSqlCdcLogService(IScriptExecuter scriptExecuter)
        {
            ScriptExecuter = scriptExecuter;
        }

        public IScriptExecuter ScriptExecuter { get; set; }

        public override async Task<IList<ICdcLog>> GetAllAsync(CancellationToken token = default)
        {
            var logs = new List<ICdcLog>();
            await ScriptExecuter.ReadAsync("SELECT * FROM pg_ls_dir('pg_wal');", (s, e) =>
            {
                while (e.Reader.Read())
                {
                    logs.Add(new CdcLog(e.Reader.GetString(0),null));
                }
                return Task.CompletedTask;
            }, token: token);
            return logs;
        }

        public override async Task<ICdcLog?> GetLastAsync(CancellationToken token = default)
        {
            ICdcLog? log = null;
            await ScriptExecuter.ReadAsync(@"SELECT 
		pg_current_logfile() AS logfile,
    pg_current_wal_lsn() AS current_lsn,
		pg_current_wal_insert_lsn() AS insert_lsn,
		pg_current_wal_flush_lsn() AS flush_lsn,
		pg_current_snapshot() AS snapshot,
		pg_current_xact_id_if_assigned() AS xact_id_if_assigned,
		pg_current_xact_id() AS xact_id,
    pg_size_pretty(pg_current_wal_insert_lsn() - pg_current_wal_lsn()) AS wal_size;
", (s, e) =>
            {
                if (e.Reader.Read())
                {
                    log = new CdcLog(e.Reader.GetString(0), (ulong)e.Reader.GetInt64(7));
                    SetRecords(e.Reader, log);
                }
                return Task.CompletedTask;
            }, token: token);
            return log;
        }
    }
}
