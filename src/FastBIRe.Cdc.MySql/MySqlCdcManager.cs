using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.MySql.Checkpoints;
using MySqlCdc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.MySql
{
#if false
    //Mysql
    enforce-gtid-consistency = ON
    gtid-mode = ON

    //Mariadb
    server-id = 1
    binlog_format = ROW
    binlog_row_image = FULL
    binlog_annotate_row_events = on
    log_bin=ON
    binlog_row_metadata = full
#endif
    public class MySqlCdcManager : ICdcManager
    {
        public MySqlCdcManager(IDbScriptExecuter scriptExecuter, MySqlCdcModes mode)
        {
            ScriptExecuter = scriptExecuter;
            Mode = mode;
        }

        public IScriptExecuter ScriptExecuter { get; }

        public MySqlCdcModes Mode { get; }

        public bool IsMariaDB { get; set; }

        public CdcOperators SupportCdcOperators => CdcOperators.WithoutEnableDisable;

        public Task<bool> IsDatabaseCdcEnableAsync(string databaseName, CancellationToken token = default)
        {
            return IsLogBinOnAsync(token);
        }
        public Task<bool> IsTableCdcEnableAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            return IsLogBinOnAsync(token);
        }

        protected Task<bool> IsLogBinOnAsync(CancellationToken token = default)
        {
            if (Mode == MySqlCdcModes.Gtid)
            {
                return ScriptExecuter.ReadResultAsync("SELECT @@GTID_MODE", (s, r) =>
                {
                    if (r.Reader.Read())
                    {
                        return Task.FromResult(string.Equals(r.Reader.GetString(1), "on", StringComparison.OrdinalIgnoreCase));
                    }
                    return Task.FromResult(false);
                });
            }
            return ScriptExecuter.ReadResultAsync("SHOW GLOBAL VARIABLES LIKE 'log_bin'", (s, r) =>
            {
                if (r.Reader.Read())
                {
                    return Task.FromResult(string.Equals(r.Reader.GetString(1), "on", StringComparison.OrdinalIgnoreCase));
                }
                return Task.FromResult(false);
            }, token: token);

        }
        public Task<ICdcListener> GetCdcListenerAsync(MySqlGetCdcListenerOptions options, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var client = new BinlogClient(options.ReplicaOptionsAction);
            return Task.FromResult<ICdcListener>(new MySqlCdcListener(client, options, Mode));
        }

        Task<ICdcListener> ICdcManager.GetCdcListenerAsync(IGetCdcListenerOptions options, CancellationToken token)
        {
            return GetCdcListenerAsync((MySqlGetCdcListenerOptions)options, token);
        }

        public async Task<DbVariables> GetCdcVariablesAsync(CancellationToken token = default)
        {
            var var = new MySqlVariables();
            await ScriptExecuter.ReadAsync("SELECT @@GTID_MODE,@@ENFORCE_GTID_CONSISTENCY", (s, r) =>
            {
                if (r.Reader.Read())
                {
                    var[MySqlVariables.GtidModeKey] = r.Reader.GetString(0);
                    var[MySqlVariables.EnfprceGtodConsistencyKey] = r.Reader.GetString(1);
                }
                return Task.CompletedTask;
            }, token: token);
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

        public Task<bool?> TryEnableDatabaseCdcAsync(string databaseName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<bool?> TryEnableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<bool?> TryDisableDatabaseCdcAsync(string databaseName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<bool?> TryDisableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<ICheckpoint?> GetLastCheckpointAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            if (IsMariaDB)
            {
                return ScriptExecuter.ReadResultAsync("SELECT @@GLOBAL.gtid_binlog_pos;", static (o, e) =>
                {
                    ICheckpoint? checkpoint = null;
                    if (e.Reader.Read())
                    {
                        var str = e.Reader.GetString(0);
                        checkpoint = MySqlCheckpoint.Parse(str, true);
                    }
                    return Task.FromResult(checkpoint);
                }, token: token);
            }
            return ScriptExecuter.ReadResultAsync("SHOW MASTER STATUS", static (e, o) =>
            {
                ICheckpoint? checkpoint = null;
                if (o.Reader.Read())
                {
                    var set = o.Reader.GetString(4);
                    checkpoint = MySqlCheckpoint.Parse(set, false);
                }
                return Task.FromResult(checkpoint);
            }, token: token);
        }
    }
}
