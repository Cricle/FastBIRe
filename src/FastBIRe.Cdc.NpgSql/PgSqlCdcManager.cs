using DatabaseSchemaReader.DataSchema;
using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.NpgSql.Checkpoints;

namespace FastBIRe.Cdc.NpgSql
{
    public class PgSqlCdcManager : ICdcManager
    {
        public const string SlotTail = "_slot";
        public const string PubTail = "_pub";

        public PgSqlCdcManager(IDbScriptExecuter scriptExecuter)
        {
            ScriptExecuter = scriptExecuter;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public CdcOperators SupportCdcOperators => CdcOperators.All & ~CdcOperators.EnableDatabaseCdc & ~CdcOperators.DisableDatabaseCdc;

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
                if (r.Reader.Read())
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
            return ScriptExecuter.ExistsAsync($"SELECT 1 FROM pg_catalog.pg_publication_tables WHERE tablename='{tableName}';", token: token);
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

        public Task<bool?> TryEnableDatabaseCdcAsync(string databaseName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }

        public async Task<bool?> TryEnableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            var exists = await IsTableCdcEnableAsync(databaseName, tableName, token);
            if (!exists)
            {
                exists = await IsReplicationSlotsExistsAsync(databaseName, $"{tableName}{SlotTail}", token);
            }
            if (exists)
            {
                await TryDisableTableCdcAsync(databaseName, tableName, token);
            }
            var result = await ScriptExecuter.ExistsAsync($"CREATE PUBLICATION \"{tableName}{PubTail}\" FOR TABLE \"{tableName}\";", token: token);
            result &= await ScriptExecuter.ExistsAsync($"SELECT * FROM pg_create_logical_replication_slot({GetSlotName(tableName)}, 'pgoutput');", token: token);
            result &= await ScriptExecuter.ExistsAsync($"ALTER TABLE \"{tableName}\" REPLICA IDENTITY FULL;", token: token);
            return result;
        }

        public Task<bool?> TryDisableDatabaseCdcAsync(string databaseName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }

        public async Task<bool?> TryDisableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            var exists = await IsTableCdcEnableAsync(databaseName, tableName, token);
            if (exists)
            {
                await ScriptExecuter.ExistsAsync($"DROP PUBLICATION \"{tableName}{PubTail}\";", token: token);
            }
            exists = await IsReplicationSlotsExistsAsync(databaseName, $"{tableName}{SlotTail}", token);
            if (exists)
            {
                await ScriptExecuter.ExistsAsync($"SELECT pg_drop_replication_slot({GetSlotName(tableName)});", token: token);
            }
            return true;
        }
        public static string? GetPubName(string tableName)
        {
            return SqlType.PostgreSql.WrapValue(tableName + PubTail);
        }
        public static string? GetSlotName(string tableName)
        {
            return SqlType.PostgreSql.WrapValue(tableName + SlotTail);
        }
        public Task<bool> IsReplicationSlotsExistsAsync(string database,string name, CancellationToken token = default)
        {
            return ScriptExecuter.ExistsAsync($"SELECT 1 FROM pg_replication_slots WHERE database={SqlType.PostgreSql.WrapValue(database)} AND slot_name={SqlType.PostgreSql.WrapValue(name)};", token: token);
        }
    }
}
