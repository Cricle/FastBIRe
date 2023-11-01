using DatabaseSchemaReader.DataSchema;
using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.Mssql.Checkpoints;
using Microsoft.Data.SqlClient;

namespace FastBIRe.Cdc.Mssql
{
    public class MssqlCdcManager : ICdcManager, IDisposable
    {
        public MssqlCdcManager(Func<IDbScriptExecuter> scriptExecuterFactory)
        {
            ScriptExecuterFactory = scriptExecuterFactory;
            ScriptExecuter = scriptExecuterFactory();
            disposables.Add(ScriptExecuter);
        }

        private readonly IList<IDisposable> disposables = new List<IDisposable>();

        public IDbScriptExecuter ScriptExecuter { get; }

        public Func<IDbScriptExecuter> ScriptExecuterFactory { get; }

        public SqlConnection Connection => (SqlConnection)ScriptExecuter.Connection;

        public CdcOperators SupportCdcOperators => CdcOperators.All;

        public Task<ICdcListener> GetCdcListenerAsync(MssqlGetCdcListenerOptions options, CancellationToken token = default)
        {
            var executer = ScriptExecuterFactory();
            disposables.Add(executer);
            return Task.FromResult<ICdcListener>(new MssqlCdcListener(this, options));
        }
        Task<ICdcListener> ICdcManager.GetCdcListenerAsync(IGetCdcListenerOptions options, CancellationToken token = default)
        {
            return GetCdcListenerAsync((MssqlGetCdcListenerOptions)options, token);
        }
        public Task<ICdcLogService> GetCdcLogServiceAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICdcLogService>(new MssqlCdcLogService(ScriptExecuter));
        }
        public Task<byte[]?> GetMinLsnAsync(string tableName, CancellationToken token = default)
        {
            return GetLsnAsync($@"DECLARE @from_lsn binary (10)
SET @from_lsn = sys.fn_cdc_get_min_lsn('{tableName}')
IF @from_lsn = 0x00000000000000000000
	SET @from_lsn = (SELECT TOP 1 __$start_lsn FROM [cdc].[dbo_{tableName}_CT] ORDER BY __$start_lsn)
SELECT @from_lsn", token);
        }
        public async Task<MssqlLsn> GetMaxLSNAsync(CancellationToken token = default)
        {
            var buff =await GetLsnAsync("SELECT sys.fn_cdc_get_max_lsn()", token);
            return MssqlLsn.Create(buff);
        }

        private async Task<byte[]?> GetLsnAsync(string script, CancellationToken token = default)
        {
            byte[]? lsn = null;
            await ScriptExecuter.ReadAsync(script, (s, r) =>
            {
                if (r.Reader.Read())
                {
                    lsn = (byte[])r.Reader[0];
                }
                return Task.CompletedTask;
            }, token: token);
            return lsn;
        }

        public async Task<DbVariables> GetCdcVariablesAsync(CancellationToken token = default)
        {
            var var = new MssqlVariables();
            await ScriptExecuter.ReadAsync("EXEC master.dbo.xp_servicecontrol N'QUERYSTATE',N'SQLSERVERAGENT'", (s, r) =>
            {
                while (r.Reader.Read())
                {
                    var[MssqlVariables.AgentStateKey] = r.Reader.GetString(0);
                }
                return Task.CompletedTask;
            }, token: token);
            return var;
        }

        public Task<bool> IsDatabaseCdcEnableAsync(string databaseName, CancellationToken token = default)
        {
            return ScriptExecuter.ExistsAsync($"SELECT 1 from sys.databases where name ='{databaseName}' AND is_cdc_enabled = 1", token: token);
        }
        public async Task<IList<string>> GetEnableCdcTableNamesAsync(CancellationToken token = default)
        {
            var var = new List<string>();
            await ScriptExecuter.ReadAsync("SELECT name from sys.tables where is_tracked_by_cdc = 1", (s, r) =>
            {
                while (r.Reader.Read())
                {
                    var.Add(r.Reader.GetString(0));
                }
                return Task.CompletedTask;
            }, token: token);
            return var;
        }
        public Task<bool> IsTableCdcEnableAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            return ScriptExecuter.ExistsAsync($"SELECT 1 from sys.tables where name ='{tableName}' AND is_tracked_by_cdc = 1", token: token);
        }

        public void Dispose()
        {
            foreach (var item in disposables)
            {
                item.Dispose();
            }
            disposables.Clear();
        }

        public Task<ICheckPointManager> GetCdcCheckPointManagerAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICheckPointManager>(MssqlCheckpointManager.Instance);
        }

        public async Task<bool> IsDatabaseSupportAsync(CancellationToken token = default)
        {
            var var = await GetCdcVariablesAsync(token);
            var state = var.GetOrDefault(MssqlVariables.AgentStateKey);
            if (state!=null&&state.StartsWith("running", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public async Task<bool?> TryEnableDatabaseCdcAsync(string databaseName, CancellationToken token = default)
        {
            var dbName = ScriptExecuter.Connection.Database;
            var scripts = $@"
USE [{databaseName}];
EXEC sys.sp_cdc_enable_db;
USE [{dbName}];
";
            await ScriptExecuter.ExecuteAsync(scripts,token:token);
            return true;
        }

        public async Task<bool?> TryEnableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            if (!await IsDatabaseCdcEnableAsync(databaseName, token))
            {
                return false;
            }
            if (await IsTableCdcEnableAsync(databaseName,tableName))
            {
                return true;
            }
            var dbName = ScriptExecuter.Connection.Database;
            var scripts = $@"
USE [{databaseName}];
EXEC sys.sp_cdc_enable_table
  @source_schema = 'dbo',
  @source_name = {SqlType.SqlServer.WrapValue(tableName)},
  @role_name = NULL;
USE [{dbName}];
";
            await ScriptExecuter.ExecuteAsync(scripts, token: token);
            return true;
        }

        public async Task<bool?> TryDisableDatabaseCdcAsync(string databaseName, CancellationToken token = default)
        {
            if (await IsDatabaseCdcEnableAsync(databaseName, token))
            {
                return true;
            }
            var dbName = ScriptExecuter.Connection.Database;
            var scripts = $@"
USE [{databaseName}];
EXEC sys.sp_cdc_disable_db;
USE [{dbName}];
";
            await ScriptExecuter.ExecuteAsync(scripts, token: token);
            return true;
        }

        public async Task<bool?> TryDisableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            var dbName = ScriptExecuter.Connection.Database;
            var scripts = $@"
USE [{databaseName}];
EXEC sys.sp_cdc_disable_table
  @source_schema = 'dbo',
  @source_name = {SqlType.SqlServer.WrapValue(tableName)},
  @capture_instance = 'ALL';
USE [{dbName}];
";
            await ScriptExecuter.ExecuteAsync(scripts, token: token);
            return true;
        }
    }
}
