﻿using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.NpgSql.Checkpoints;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using NpgsqlTypes;

namespace FastBIRe.Cdc.NpgSql
{
    public class PgSqlCdcManager : ICdcManager
    {
        public PgSqlCdcManager(IDbScriptExecuter scriptExecuter)
        {
            ScriptExecuter = scriptExecuter;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public Task<ICdcListener> GetCdcListenerAsync(PgSqlGetCdcListenerOptions options, CancellationToken token = default)
        {
            return Task.FromResult<ICdcListener>(new PgSqlCdcListener(options));
        }
        Task<ICdcListener> ICdcManager.GetCdcListenerAsync(IGetCdcListenerOptions options, CancellationToken token)
        {
            return GetCdcListenerAsync((PgSqlGetCdcListenerOptions)options, token);
        }

        public Task<ICdcLogService> GetCdcLogServiceAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICdcLogService>(new PgSqlCdcLogService(ScriptExecuter));
        }

        public async Task<DbVariables> GetCdcVariablesAsync(CancellationToken token = default)
        {
            var var = new PgSqlVariables();
            await ScriptExecuter.ReadAsync("SHOW wal_level;", (s, r) =>
            {
                while (r.Reader.Read())
                {
                    var["wal_level"] = r.Reader.GetString(0);
                }
                return Task.CompletedTask;
            }, token: token);
            return var;

        }

        public async Task<bool> IsDatabaseCdcEnableAsync(string databaseName, CancellationToken token = default)
        {
            var val = (PgSqlVariables)await GetCdcVariablesAsync(token);
            return val.WalLevel == PgSqlWalLevel.Logical;
        }

        public Task<bool> IsTableCdcEnableAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            return ScriptExecuter.ExistsAsync($"SELECT 1 FROM pg_catalog.pg_publication_tables WHERE tablename='{tableName}';", token);
        }

        public Task<ICheckPointManager> GetCdcCheckPointManagerAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICheckPointManager>(PgSqlCheckpointManager.Instance);
        }

        public async Task<bool> IsDatabaseSupportAsync(CancellationToken token = default)
        {
            var var = await GetCdcVariablesAsync(token);
            var walLevel = string.Equals(var.GetOrDefault("wal_level"), "logical", StringComparison.OrdinalIgnoreCase);
            return walLevel;
        }
    }
}
