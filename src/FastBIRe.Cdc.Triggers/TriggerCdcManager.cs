using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Builders;
using DatabaseSchemaReader.SqlGen;
using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.Mssql;
using FastBIRe.Cdc.Triggers.Checkpoints;
using FastBIRe.Comparing;
using FastBIRe.Naming;
using FastBIRe.Triggering;
using System.Data;

namespace FastBIRe.Cdc.Triggers
{
    public class TriggerCdcManager : ICdcManager
    {
        public const string TimeColumn = "__$time";
        public const string ActionsColumn = "__$actions";
        public const string OkColumn = "__$ok";
        public const string IdColumn = "__$id";

        internal static readonly string[] systemColumns = new[] { TimeColumn, ActionsColumn, OkColumn, IdColumn };

        private static readonly TriggerTypes[] forDbActions = new TriggerTypes[]
        {
             TriggerTypes.AfterInsert,
             TriggerTypes.BeforeUpdate,
             TriggerTypes.AfterUpdate,
             TriggerTypes.BeforeDelete
        };

        public static readonly INameGenerator DefaultAffectTableNameGenerator = new RegexNameGenerator("{0}_affect");

        public static readonly INameGenerator DefaultAffectTriggerNameGenerator = new RegexNameGenerator("{0}_affect_{1}");
        public TriggerCdcManager(IDbScriptExecuter scriptExecuter)
            : this(scriptExecuter, DefaultAffectTableNameGenerator, DefaultAffectTriggerNameGenerator, Triggering.TriggerWriter.Default, SqlComparer.Instance)
        {

        }
        public TriggerCdcManager(IDbScriptExecuter scriptExecuter, INameGenerator affectTableNameGenerator, INameGenerator affectTriggerNameGenerator, ITriggerWriter triggerWriter, IEqualityComparer<string> sqlComparer)
        {
            ScriptExecuter = scriptExecuter;
            Reader = new DatabaseReader(scriptExecuter.Connection) { Owner = ScriptExecuter.Connection.Database };
            SqlType = Reader.SqlType!.Value;
            TableHelper = SqlType.GetTableHelper()!;
            FunctionMapper = FunctionMapper.Get(SqlType)!;
            AffectTableNameGenerator = affectTableNameGenerator;
            AffectTriggerNameGenerator = affectTriggerNameGenerator;
            TriggerWriter = triggerWriter;
            SqlEqualityComparer = sqlComparer;
        }

        public CdcOperators SupportCdcOperators => CdcOperators.All;

        public IDbScriptExecuter ScriptExecuter { get; }

        public SqlType SqlType { get; }

        public DatabaseReader Reader { get; }

        public TableHelper TableHelper { get; }

        public ITriggerWriter TriggerWriter { get; }

        public FunctionMapper FunctionMapper { get; }

        public INameGenerator AffectTableNameGenerator { get; }

        public INameGenerator AffectTriggerNameGenerator { get; }

        public IEqualityComparer<string> SqlEqualityComparer { get; }

        public Task<ICheckPointManager> GetCdcCheckPointManagerAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICheckPointManager>(TriggerCheckpointManager.Instance);
        }
        public Task<ICdcListener> GetCdcListenerAsync(TriggerGetCdcListenerOptions options, CancellationToken token = default)
        {
            return Task.FromResult<ICdcListener>(new TriggerCdcListener(options));
        }
        Task<ICdcListener> ICdcManager.GetCdcListenerAsync(IGetCdcListenerOptions options, CancellationToken token)
        {
            return GetCdcListenerAsync((TriggerGetCdcListenerOptions)options, token);
        }

        public Task<ICdcLogService> GetCdcLogServiceAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICdcLogService>(TriggerCdcLogService.Instance);
        }

        public Task<DbVariables> GetCdcVariablesAsync(CancellationToken token = default)
        {
            return Task.FromResult(new DbVariables());
        }

        public Task<bool> IsDatabaseCdcEnableAsync(string databaseName, CancellationToken token = default)
        {
            return Task.FromResult(SqlType != SqlType.DuckDB);
        }

        public Task<bool> IsDatabaseSupportAsync(CancellationToken token = default)
        {
            return Task.FromResult(SqlType != SqlType.DuckDB);
        }

        public Task<int> RemoveOkedAsync(string tableName, int? batchSize,CancellationToken token = default)
        {
            var tableHelper = SqlType.GetTableHelper()!;
            var limit = tableHelper.Pagging(null, batchSize);
            var sql = $"DELETE FROM {SqlType.Wrap(tableName)} WHERE {SqlType.Wrap(OkColumn)} = {SqlType.WrapValue(false)} {limit};";
            return ScriptExecuter.ExecuteAsync(sql, token: token);
        }

        public Task<bool> IsTableCdcEnableAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            var targetTable = Reader.Table(tableName, ReadTypes.Columns | ReadTypes.Pks| ReadTypes.Triggers, token);
            if (targetTable == null)
            {
                throw new ArgumentException($"The table {tableName} not found!");
            }
            var affectTableName = AffectTableNameGenerator.Create(new[] { tableName });
            var table = Reader.Table(affectTableName, ReadTypes.Columns);
            var ok = table != null;
            if (ok)
            {
                for (int i = 0; i < forDbActions.Length; i++)
                {
                    var affectTriggerName = AffectTriggerNameGenerator.Create(new object[] { tableName, forDbActions[i].ToString() });
                    var triggerBody = string.Join("\n", CreateTriggerScripts(tableName,affectTableName, affectTriggerName, targetTable, forDbActions[i],true));
                    ok &= targetTable.Triggers.Any(x => x.Name == affectTriggerName && (SqlType == SqlType.PostgreSql || SqlEqualityComparer.Equals(x.TriggerBody, triggerBody)));
                    if (!ok)
                    {
                        break;
                    }
                }
            }

            return Task.FromResult(ok);
        }
        private IEnumerable<string> CreateTriggerScripts(string targetTable,string affectTableName, string triggerName, DatabaseTable table, TriggerTypes action,bool onlyBody)
        {
            var columnsOnly = table.Columns.OrderBy(x => x.Name).Select(x => SqlType.Wrap(x.Name));
            var columns = columnsOnly.Concat(systemColumns.Select(x=> SqlType.Wrap(x)));

            var columnOnlyJoined = string.Join(", ", columnsOnly);
            var columnJoined = string.Join(", ", columns);
            var key = "NEW";
            if (action== TriggerTypes.AfterDelete||action== TriggerTypes.BeforeDelete)
            {
                if (SqlType== SqlType.SqlServer||SqlType== SqlType.SqlServerCe)
                {
                    key = "DELETED";
                }
                else
                {
                    key = "OLD";
                }
            }
            var values = table.Columns.OrderBy(x => x.Name).Select(x => $"{key}.{SqlType.Wrap(x.Name)}").Concat(new string[] { FunctionMapper.NowWithMill(), SqlType.WrapValue((int)action), SqlType.WrapValue(false),FunctionMapper.GuidBinary() });
            var valueJoined = string.Join(", ", values);
            string body = string.Empty;
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    body = $"INSERT INTO {SqlType.Wrap(affectTableName)}({columnJoined}) SELECT {columnOnlyJoined}, {FunctionMapper.NowWithMill()}, {SqlType.WrapValue((int)action)},{SqlType.WrapValue(false)},{FunctionMapper.GuidBinary()} FROM INSERTED;";
                    break;
                case SqlType.MySql:
                case SqlType.SQLite:
                case SqlType.PostgreSql:
                    body = $"INSERT INTO {SqlType.Wrap(affectTableName)}({columnJoined}) VALUES ({valueJoined});";
                    break;
                case SqlType.Db2:
                case SqlType.DuckDB:
                case SqlType.Oracle:
                default:
                    throw new NotSupportedException(SqlType.ToString());
            }
            if (onlyBody)
            {
                if (SqlType== SqlType.MySql)
                {
                    body = $"BEGIN {body} END;";
                }
                return new string[] { body };
            }
            return TriggerWriter.Create(SqlType, triggerName, action, targetTable, body, null);

        }

        public Task<bool?> TryDisableDatabaseCdcAsync(string databaseName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }

        public async Task<bool?> TryDisableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            var affectTableName = AffectTableNameGenerator.Create(new[] { tableName });
            var scripts = new List<string>(0);
            var table = Reader.Table(tableName, ReadTypes.Triggers);
            if (table != null)
            {
                //Drop all triggers
                foreach (var item in table.Triggers)
                {
                    scripts.AddRange(TriggerWriter.Drop(SqlType, item.Name, tableName));
                }
                if (Reader.TableExists(affectTableName))
                {
                    scripts.Add($"DROP TABLE {SqlType.Wrap(affectTableName)};");
                }
                await ScriptExecuter.ExecuteBatchAsync(scripts, token: token);
                return true;
            }
            return false;
        }

        public Task<bool?> TryEnableDatabaseCdcAsync(string databaseName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(SqlType != SqlType.DuckDB);
        }
        private string GetDateTimeType()
        {
            switch (SqlType)
            {
                case SqlType.MySql:
                case SqlType.PostgreSql:
                case SqlType.DuckDB:
                    return $"datetime(3)";
                case SqlType.SQLite:
                    return "TEXT";
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"datetime2";
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    throw new NotSupportedException(SqlType.ToString());
            }
        }
        private string GetUUIDType()
        {
            switch (SqlType)
            {
                case SqlType.MySql:
                    return $"BINARY(16)";
                case SqlType.SQLite:
                    return "BLOB";
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"UNIQUEIDENTIFIER";
                case SqlType.PostgreSql:
                case SqlType.DuckDB:
                    return "uuid";
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    throw new NotSupportedException(SqlType.ToString());
            }
        }

        public DatabaseTable CreateAffectTable(DatabaseTable targetTable,string affectTableName)
        {
            var table = new DatabaseTable { Name = affectTableName };
            foreach (var item in targetTable.Columns)
            {
                table.AddColumn(item);
            }
            table.PrimaryKey = null;
            table.AddColumn(TimeColumn, GetDateTimeType(), x => x.Nullable = false);
            table.AddColumn(ActionsColumn, DbType.Byte, x => x.Nullable = false);
            table.AddColumn(OkColumn, DbType.Boolean, x => x.Nullable = false);
            table.AddColumn(IdColumn, GetUUIDType(), x => x.Nullable = false).AddPrimaryKey($"PK_{affectTableName}");
            table.AddIndex(new DatabaseIndex
            {
                Name = $"IX_{affectTableName}_{TimeColumn}",
                Columns = { table.FindColumn(TimeColumn) }
            });
            return table;
        }

        public async Task<bool?> TryEnableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            var isEnable = await IsTableCdcEnableAsync(databaseName, tableName, token);
            if (isEnable)
            {
                return true;
            }
            await TryDisableTableCdcAsync(databaseName, tableName, token);
            var targetTable = Reader.Table(tableName, ReadTypes.Columns, token);
            var affectTableName = AffectTableNameGenerator.Create(new[] { tableName });
            var table = CreateAffectTable(targetTable, affectTableName);
            var createTableScript = new DdlGeneratorFactory(SqlType).TableGenerator(table).Write();
            var scripts = new List<string> { createTableScript };
            if (table != null)
            {
                for (int i = 0; i < forDbActions.Length; i++)
                {
                    var affectTriggerName = AffectTriggerNameGenerator.Create(new object[] { tableName, forDbActions[i] });
                    scripts.AddRange(CreateTriggerScripts(tableName,affectTableName, affectTriggerName, targetTable, forDbActions[i], false));
                }
                await ScriptExecuter.ExecuteBatchAsync(scripts, token: token);
                return true;
            }
            return false;
        }
    }
}
