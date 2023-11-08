﻿using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.MySql.Checkpoints;
using MySqlCdc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.MySql
{
#if false
    enforce-gtid-consistency = ON
    gtid-mode = ON
#endif
    public class MySqlCdcManager : ICdcManager
    {
        public MySqlCdcManager(IScriptExecuter scriptExecuter, MySqlCdcModes mode)
        {
            ScriptExecuter = scriptExecuter;
            Mode = mode;
        }

        public IScriptExecuter ScriptExecuter { get; }

        public MySqlCdcModes Mode { get; }

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
            if (Mode== MySqlCdcModes.Gtid)
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
            var client = new BinlogClient(options.ReplicaOptionsAction);
            return Task.FromResult<ICdcListener>(new MySqlCdcListener(client, options,Mode));
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
    }
}
