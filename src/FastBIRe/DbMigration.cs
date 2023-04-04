﻿using Ao.Stock.Mirror;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using System.Data;
using System.Data.Common;

namespace FastBIRe
{
    public enum CompareWithModifyResultTypes
    {
        Succeed = 0,
        NoSuchTable = 1,
    }
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
            var table = Reader.Table(options.Table);
            if (table == null)
            {
                return 0;
            }
            var tableIndexs = table.Indexes;
            var index = tableIndexs.FirstOrDefault(x => x.Name == options.IndexName);
            if (index != null && !options.DuplicationNameReplace)
            {
                if (options.IgnoreOnSequenceEqualName &&
                    options.Columns.Count() == tableIndexs.Count &&
                    options.Columns.SequenceEqual(tableIndexs.Select(x => x.Name)))
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
            var createIndexSql = TableHelper.CreateIndex(options.IndexName, options.Table, options.Columns.ToArray());
            Log("Run drop index {0} sql\n{1}", options.IndexName, createIndexSql);
            var createRes = await Connection.ExecuteNonQueryAsync(createIndexSql, timeout: CommandTimeout, token: token);
            Log("Create index {0} result {1}", options.IndexName, createRes);
            return createRes;
        }
        public async Task EnsureDatabaseCreatedAsync(CancellationToken token = default)
        {
            var adapter = GetSQLDatabaseCreateAdapter();
            if (adapter == null)
            {
                throw new NotSupportedException($"Not support {SqlType} to EnsureDatabaseCreatedAsync");
            }
            if (SqlType == SqlType.PostgreSql)
            {
                var hasDbSql = $"SELECT 1 FROM pg_database WHERE datname = '{Database}'";
                Log("Check db sql \n{0}", hasDbSql);
                var hasDb = await Connection.ExecuteReadCountAsync(hasDbSql, commandTimeout: CommandTimeout, token: token);
                if (hasDb <= 0)
                {
                    var createSql = adapter.GenericCreateDatabaseSql(Database);
                    Log("Create db sql \n{0}", createSql);
                    var createRes = await Connection.ExecuteNonQueryAsync(createSql, timeout: CommandTimeout, token: token);
                    Log("Sync database result {0}", createRes);
                }
            }
            else
            {
                var createIfNotExistsSql = adapter.GenericCreateDatabaseIfNotExistsSql(Database);
                Log("Create if not exists with sql \n{0}", createIfNotExistsSql);
                var createRes = await Connection.ExecuteNonQueryAsync(createIfNotExistsSql, timeout: CommandTimeout, token: token);
                Log("Sync database result {0}", createRes);
            }
        }
    }
}
