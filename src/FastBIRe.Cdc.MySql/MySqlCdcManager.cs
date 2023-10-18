using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.MySql.Checkpoints;
using MySqlCdc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.MySql
{
    public class MySqlCdcManager : ICdcManager
    {
        public MySqlCdcManager(IScriptExecuter scriptExecuter, Action<ReplicaOptions> replicaAction)
        {
            ScriptExecuter = scriptExecuter;
            ReplicaAction = replicaAction;
            BinlogClient = new BinlogClient(ReplicaAction);
        }

        public IScriptExecuter ScriptExecuter { get; }

        public Action<ReplicaOptions> ReplicaAction { get; }

        public BinlogClient BinlogClient { get; }

        public Task<bool> IsDatabaseCdcEnableAsync(string databaseName, CancellationToken token = default)
        {
            return IsLogBinOnAsync(token);
        }
        public Task<bool> IsTableCdcEnableAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            return IsLogBinOnAsync(token);
        }

        protected async Task<bool> IsLogBinOnAsync(CancellationToken token = default)
        {
            var on = false;
            await ScriptExecuter.ReadAsync("SHOW GLOBAL VARIABLES LIKE 'log_bin'", (s, r) =>
            {
                while (r.Reader.Read())
                {
                    on = string.Equals(r.Reader.GetString(1), "on", StringComparison.OrdinalIgnoreCase);
                }
                return Task.CompletedTask;
            }, token: token);
            return on;

        }
        public Task<ICdcListener> GetCdcListenerAsync(MySqlGetCdcListenerOptions options, CancellationToken token = default)
        {
            return Task.FromResult<ICdcListener>(new MySqlCdcListener(BinlogClient, options));
        }

        Task<ICdcListener> ICdcManager.GetCdcListenerAsync(IGetCdcListenerOptions options, CancellationToken token = default)
        {
            return GetCdcListenerAsync((MySqlGetCdcListenerOptions)options, token);
        }

        public async Task<DbVariables> GetCdcVariablesAsync(CancellationToken token = default)
        {
            var var = new MySqlVariables();
            await ScriptExecuter.ReadAsync("SHOW GLOBAL VARIABLES LIKE '%bin%'", (s, r) =>
            {
                while (r.Reader.Read())
                {
                    var[r.Reader.GetString(0)] = r.Reader.GetString(1);
                }
                return Task.CompletedTask;
            }, token: token);
            return var;
        }

        public Task<ICdcLogService> GetCdcLogServiceAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICdcLogService>(new MySqlCdcLogService(ScriptExecuter));
        }
        public Task<ICheckPointManager> GetCdcCheckPointManagerAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICheckPointManager>(MysqlCheckpointManager.Instance);
        }

        public async Task<bool> IsDatabaseSupportAsync(CancellationToken token = default)
        {
            var var = await GetCdcVariablesAsync(token);
            var logBin = string.Equals(var.GetOrDefault("log_bin"), "ON", StringComparison.OrdinalIgnoreCase);
            var binlogFormat = string.Equals(var.GetOrDefault("binlog_format"), "ROW", StringComparison.OrdinalIgnoreCase);
            return logBin && binlogFormat;
        }
    }
}
