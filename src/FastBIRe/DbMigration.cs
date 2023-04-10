using Ao.Stock.Mirror;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace FastBIRe
{
    public class DbMigration : IDisposable
    {
        public DbMigration(DbConnection connection)
            : this(connection, connection.Database)
        {

        }
        public DbMigration(DbConnection connection, string database)
        {
            Connection = connection;
            Reader = new DatabaseReader(connection) { Owner = database };
            TableHelper = new TableHelper(SqlType);
            DdlGeneratorFactory = new DdlGeneratorFactory(SqlType);
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public Action<string>? Logger { get; set; }

        public string Database { get; }

        public DatabaseReader Reader { get; }

        public DbConnection Connection { get; }

        public TableHelper TableHelper { get; }

        public DdlGeneratorFactory DdlGeneratorFactory { get; }

        public SqlType SqlType => Reader.SqlType!.Value;

        public int CommandTimeout { get; set; } = 10 * 60;
        public async Task<int> ExecuteNonQueryAsync(IEnumerable<string> sqls, CancellationToken token = default)
        {
            var res = 0;
            foreach (var item in sqls)
            {
                token.ThrowIfCancellationRequested();
                res+=await ExecuteNonQueryAsync(item, token);
            }
            return res;
        }
        public async Task<int> ExecuteNonQueryAsync(string sql,CancellationToken token=default)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return 0;
            }
            Log("Executing sql \n{0}", sql);
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = CommandTimeout;
                return await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            Reader.Dispose();
            Connection.Dispose();
            GC.SuppressFinalize(this);
        }

        protected void Log(string message, params object?[] args)
        {
            if (args == null)
            {
                Logger?.Invoke(message);
            }
            else
            {
                Logger?.Invoke(string.Format(message, args));
            }
        }

        public MergeHelper GetMergeHelper()
        {
            return new MergeHelper(SqlType);
        }

        public ISQLDatabaseCreateAdapter? GetSQLDatabaseCreateAdapter()
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return SQLDatabaseCreateAdapter.SqlServer;
                case SqlType.Oracle:
                    return SQLDatabaseCreateAdapter.Oracle;
                case SqlType.MySql:
                    return SQLDatabaseCreateAdapter.MySql;
                case SqlType.SQLite:
                    return SQLDatabaseCreateAdapter.Sqlite;
                case SqlType.PostgreSql:
                    return SQLDatabaseCreateAdapter.PostgreSql;
                default:
                    return null;
            }
        }
        public CompareWithModifyResult CompareWithModify(string tableId, Action<DatabaseTable> modify)
        {
            var tableOld = Reader.Table(tableId);
            if (tableId==null)
            {
                return new CompareWithModifyResult { Type = CompareWithModifyResultTypes.NoSuchTable };
            }
            var tableNew = Reader.Table(tableId);
            modify(tableNew);
            var schemaOld = CloneSchema();
            schemaOld.AddTable(tableOld);
            var schemaNew = CloneSchema();
            schemaNew.AddTable(tableNew);
            return new CompareWithModifyResult
            {
                Type = CompareWithModifyResultTypes.Succeed,
                Schemas = new CompareSchemas(schemaOld, schemaNew)
            };
        }

        public DatabaseSchema CloneSchema()
        {
            return new DatabaseSchema(Reader.DatabaseSchema.ConnectionString, SqlType);
        }

        public async Task<int> SyncIndexAsync(SyncIndexOptions options, CancellationToken token = default)
        {
            //Single indexs
            var table = Reader.Table(options.Table);
            if (table == null)
            {
                return 0;
            }
            if (options.Columns==null)
            {
                throw new ArgumentException("Column must not null");
            }
            var scripts = new List<string>();
            var tableIndexs = table.Indexes;
            var nameCreator = options.IndexNameCreator ?? (s => $"IDX_{options.Table}_{s}");
            var refedIndexs= new HashSet<string>();
            foreach (var col in options.Columns)
            {
                var name = nameCreator(col);
                var oldIndex = tableIndexs.FirstOrDefault(x => x.Name == name);
                if (oldIndex!=null)
                {
                    if (oldIndex.Columns.Count != 1 || oldIndex.Columns[0].Name!=col)
                    {
                        scripts.Add(TableHelper.DropIndex(name, options.Table));
                    }
                    else
                    {
                        continue;
                    }
                }
                scripts.Add(TableHelper.CreateIndex(name, options.Table, col));
                refedIndexs.Add(name);
            }
            if (options.RemoveNotRef&&refedIndexs.Count!=0)
            {
                foreach (var item in refedIndexs)
                {
                    if (options.RemoveFilter!=null&&!options.RemoveFilter(item))
                    {
                        continue;
                    }
                    scripts.Add(TableHelper.DropIndex(item, options.Table));
                }
            }
            var res=await ExecuteNonQueryAsync(scripts, token);
            return res;
        }
        public async Task EnsureDatabaseCreatedAsync(string database,CancellationToken token = default)
        {
            var adapter = GetSQLDatabaseCreateAdapter();
            if (adapter == null)
            {
                throw new NotSupportedException($"Not support {SqlType} to EnsureDatabaseCreatedAsync");
            }
            if (SqlType == SqlType.PostgreSql)
            {
                var hasDbSql = $"SELECT 1 FROM pg_database WHERE datname = '{database}'";
                Log("Check db sql \n{0}", hasDbSql);
                var hasDb = false;
                using (var command=Connection.CreateCommand(hasDbSql))
                {
                    command.CommandTimeout = CommandTimeout;
                    using (var reader=await command.ExecuteReaderAsync(token))
                    {
                        hasDb = reader.Read();
                    }
                }
                if (!hasDb)
                {
                    var createSql = adapter.GenericCreateDatabaseSql(database);
                    Log("Create db sql \n{0}", createSql);
                    var createRes = await Connection.ExecuteNonQueryAsync(createSql, timeout: CommandTimeout, token: token);
                    Log("Sync database result {0}", createRes);
                }
            }
            else
            {
                var createIfNotExistsSql = adapter.GenericCreateDatabaseIfNotExistsSql(database);
                Log("Create if not exists with sql \n{0}", createIfNotExistsSql);
                var createRes = await Connection.ExecuteNonQueryAsync(createIfNotExistsSql, timeout: CommandTimeout, token: token);
                Log("Sync database result {0}", createRes);
            }
        }
    }
}
