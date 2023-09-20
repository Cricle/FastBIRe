using Ao.Stock.Mirror;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using System.Data.Common;

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
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public Action<string>? Logger { get; set; }

        public string Database { get; }

        private DatabaseReader? reader;
        private TableHelper? tableHelper;
        private DdlGeneratorFactory? ddlGeneratorFactory;
        private IMigrationGenerator? migrationGenerator;

        public DatabaseReader Reader => reader ??= new DatabaseReader(Connection) { Owner = Database };

        public DbConnection Connection { get; }

        public TableHelper TableHelper => tableHelper ??= new TableHelper(SqlType);

        public DdlGeneratorFactory DdlGeneratorFactory => ddlGeneratorFactory ??= new DdlGeneratorFactory(SqlType);

        public IMigrationGenerator MigrationGenerator => migrationGenerator ??= DdlGeneratorFactory.MigrationGenerator();

        public SqlType SqlType => Reader.SqlType!.Value;

        public int CommandTimeout { get; set; } = 10 * 60;
        public async Task<int> ExecuteNonQueryAsync(IEnumerable<string> sqls, CancellationToken token = default)
        {
            var res = 0;
            foreach (var item in sqls)
            {
                token.ThrowIfCancellationRequested();
                res += await ExecuteNonQueryAsync(item, token);
            }
            return res;
        }
        public async Task<int> ExecuteNonQueryAsync(string sql, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
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
        private MergeHelper? helper;

        public MergeHelper GetMergeHelper()
        {
            return helper ??= new MergeHelper(SqlType);
        }
        public SourceTableColumnBuilder GetColumnBuilder(string? sourceAlias = "a", string? destAlias = "b")
        {
            return new SourceTableColumnBuilder(GetMergeHelper(), sourceAlias, destAlias);
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
        public virtual void CleanIdentityType(IEnumerable<DatabaseColumn> columns)
        {
            foreach (var item in columns)
            {
                item.Length = null;
                item.Precision = null;
                item.Scale = null;
            }
        }
        public virtual void PrepareTable(DatabaseTable table)
        {
            foreach (var item in table.Columns)
            {
                item.DefaultValue = null;
            }
        }
        public CompareWithModifyResult CompareWithModify(string tableId, Action<DatabaseTable> modify)
        {
            var tableOld = Reader.Table(tableId);
            if (tableId == null)
            {
                return new CompareWithModifyResult { Type = CompareWithModifyResultTypes.NoSuchTable };
            }
            var tableNew = Reader.Table(tableId);
            PrepareTable(tableNew);
            PrepareTable(tableOld);
            CleanIdentityType(tableOld.Columns);
            CleanIdentityType(tableNew.Columns);
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
        public async Task<int> SyncIndexSingleAsync(SyncIndexOptions options, List<string>? outIndexNames = null, CancellationToken token = default)
        {
            //Single indexs
            var table = Reader.Table(options.Table);
            if (table == null)
            {
                return 0;
            }
            if (options.Columns == null)
            {
                throw new ArgumentException("Column must not null");
            }
            var scripts = new List<string>();
            var tableIndexs = table.Indexes;
            var nameCreator = options.IndexNameCreator ?? (s => $"IDX_{options.Table}_{s}");
            var refedIndexs = new HashSet<string>();
            var dropedIndexs = new HashSet<string>();
            foreach (var col in options.Columns)
            {
                var name = nameCreator(col);
                var oldIndex = tableIndexs.FirstOrDefault(x => x.Name == name);
                if (oldIndex != null)
                {
                    if (oldIndex.Columns.Count != 1 || oldIndex.Columns[0].Name != col)
                    {
                        if (dropedIndexs.Add(name))
                        {
                            scripts.Add(TableHelper.DropIndex(name, options.Table));
                        }
                    }
                    else
                    {
                        outIndexNames?.Add(name);
                        continue;
                    }
                }
                outIndexNames?.Add(name);
                scripts.Add(TableHelper.CreateIndex(name, options.Table, new[] { col }));
                refedIndexs.Add(name);
            }
            if (options.RemoveNotRef && refedIndexs.Count != 0)
            {
                table = Reader.Table(options.Table);
                foreach (var item in refedIndexs)
                {
                    if (options.RemoveFilter != null && !options.RemoveFilter(item))
                    {
                        continue;
                    }
                    if (dropedIndexs.Add(item) && !table.Indexes.Any(x => x.Name == item))
                    {
                        scripts.Add(TableHelper.DropIndex(item, options.Table));
                    }
                }
            }
            var res = await ExecuteNonQueryAsync(scripts, token);
            return res;
        }
        public async Task<int> SyncIndexAsync(SyncIndexOptions options, CancellationToken token = default)
        {
            var table = Reader.Table(options.Table);
            if (table == null)
            {
                return 0;
            }
            var tableIndexs = table.Indexes;
            var index = tableIndexs.FirstOrDefault(x => x.Name == options.IndexName);
            if (index != null && !options.DuplicationNameReplace)
            {
                if (options.Columns.Count() == index.Columns.Count &&
                    options.Columns.SequenceEqual(index.Columns.Select(x => x.Name)) &&
                    options.IgnoreOnSequenceEqualName)
                {
                    return 0;
                }
            }
            if (index != null)
            {
                var dropIndexSql = TableHelper.DropIndex(index.Name, index.TableName);
                Log("Run drop index {0} sql\n{1}", index.Name, dropIndexSql);
                var res = await Connection.ExecuteNonQueryAsync(dropIndexSql, timeout: CommandTimeout, token: token);
                Log("Drop index result {0}", res);
            }
            var createIndexSql = TableHelper.CreateIndex(options.IndexName, options.Table, options.Columns.Distinct().ToArray());
            Log("Run create index {0} sql\n{1}", options.IndexName, createIndexSql);
            var createRes = await Connection.ExecuteNonQueryAsync(createIndexSql, timeout: CommandTimeout, token: token);
            Log("Create index {0} result {1}", options.IndexName, createRes);
            return createRes;
        }
        public async Task EnsureDatabaseCreatedAsync(string database, CancellationToken token = default)
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
                using (var command = Connection.CreateCommand(hasDbSql))
                {
                    command.CommandTimeout = CommandTimeout;
                    using (var reader = await command.ExecuteReaderAsync(token))
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
