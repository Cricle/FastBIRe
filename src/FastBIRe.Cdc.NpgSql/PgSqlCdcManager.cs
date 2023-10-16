using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using NpgsqlTypes;

namespace FastBIRe.Cdc.NpgSql
{
    public class PgSqlCdcManager : ICdcManager
    {
        public PgSqlCdcManager(IDbScriptExecuter scriptExecuter, LogicalReplicationConnection logicalReplicationConnection , PgOutputReplicationSlot outputReplicationSlot, PgOutputReplicationOptions outputReplicationOptions, NpgsqlLogSequenceNumber? npgsqlLogSequenceNumber = null)
        {
            ScriptExecuter = scriptExecuter;
            OutputReplicationSlot = outputReplicationSlot;
            OutputReplicationOptions = outputReplicationOptions;
            NpgsqlLogSequenceNumber = npgsqlLogSequenceNumber;
            LogicalReplicationConnection = logicalReplicationConnection;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public LogicalReplicationConnection LogicalReplicationConnection { get; }

        public PgOutputReplicationSlot OutputReplicationSlot { get; }

        public PgOutputReplicationOptions OutputReplicationOptions { get; }

        public NpgsqlLogSequenceNumber? NpgsqlLogSequenceNumber { get; }

        public Task<ICdcListener> GetCdcListenerAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICdcListener>(new PgSqlCdcListener(
                LogicalReplicationConnection,
                OutputReplicationSlot,
                OutputReplicationOptions,
                NpgsqlLogSequenceNumber));
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
    }
}
