using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using DatabaseSchemaReader.Utilities;
using FastBIRe.Comparing;
using FastBIRe.Creating;
using System.Data;
using System.Data.Common;
using System.Text;

namespace FastBIRe.Farm
{
    public enum SyncResult
    {
        NoModify = 0,
        Modify = 1
    }
    public class FarmWarehouse : IDisposable
    {
        public FarmWarehouse(IDbScriptExecuter scriptExecuter)
            : this(scriptExecuter, scriptExecuter.CreateReader())
        {
        }
        public FarmWarehouse(IDbScriptExecuter scriptExecuter, DatabaseReader databaseReader)
        {
            ScriptExecuter = scriptExecuter;
            DatabaseReader = databaseReader;
            SqlType = databaseReader.SqlType!.Value;
            TableHelper = new TableHelper(SqlType);
            DatabaseCreateAdapter = SqlType.GetDatabaseCreateAdapter()!;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public DatabaseReader DatabaseReader { get; }

        public DbConnection Connection => ScriptExecuter.Connection;

        public SqlType SqlType { get; }

        public TableHelper TableHelper { get; }

        public IDatabaseCreateAdapter DatabaseCreateAdapter { get; }

        public IList<string> GetSyncScripts(DatabaseTable table, IEnumerable<int>? maskColumns = null)
        {
            var scripts = new List<string>();
            var copyTable = table.Clone();
            if (maskColumns != null)
            {
                var removes = copyTable.Columns.Where((x, i) => maskColumns.Contains(i)).ToList();
                copyTable.Columns.RemoveAll(x => !removes.Contains(x));
            }
            copyTable.SchemaOwner = DatabaseReader.Owner;
            copyTable.DatabaseSchema = DatabaseReader.DatabaseSchema;
            copyTable.Indexes = new List<DatabaseIndex>();
            copyTable.CheckConstraints.RemoveAll(x => x.ConstraintType == ConstraintType.Check);
            foreach (var item in copyTable.Columns)
            {
                item.IsAutoNumber = false;
                item.DefaultValue = null;
            }
            if (DatabaseReader.TableExists(copyTable.Name))
            {
                //Check struct
                var tb = DatabaseReader.Table(copyTable.Name);
                var structIsEquals = tb.Columns.Count == copyTable.Columns.Count &&
                    tb.Columns.All(x =>
                    {
                        var dest = copyTable.Columns.FirstOrDefault(cx => cx.Name == x.Name);
                        if (dest == null)
                        {
                            return false;
                        }
                        return DefaultDatabaseColumnComparer.Instance.Equals(dest, x);
                    });
                if (structIsEquals)
                {
                    return scripts;
                }
                scripts.Add(DatabaseCreateAdapter.DropTableIfExists(copyTable.Name));
            }
            var ddlFactory = new DdlGeneratorFactory(SqlType);
            var ddlTable = ddlFactory.TableGenerator(copyTable).Write();
            scripts.Add(ddlTable);
            return scripts;
        }
        public virtual Task<int> DeleteAsync(string tableName, CancellationToken token = default)
        {
            return ScriptExecuter.ExecuteAsync($"DELETE FROM {SqlType.Wrap(tableName)}", token: token);
        }
        public async Task<SyncResult> SyncAsync(DatabaseTable table, IEnumerable<int>? maskColumns = null, CancellationToken token = default)
        {
            var scripts = GetSyncScripts(table, maskColumns);
            if (scripts.Count != 0)
            {
                await ScriptExecuter.ExecuteBatchAsync(scripts, token: token);
                return SyncResult.Modify;
            }
            return SyncResult.NoModify;
        }
        public Task<int> SyncDataAsync(string tableName, IDataReader reader, int batchSize, CancellationToken token = default)
        {
            var columnNames = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames[i] = reader.GetName(i);
            }
            return SyncDataAsync(tableName, columnNames, reader, batchSize, token);
        }
        public virtual async Task<int> SyncDataAsync(string tableName, IEnumerable<string> columnNames, IDataReader reader, int batchSize, CancellationToken token = default)
        {
            var batchs = new object?[batchSize][];
            for (int i = 0; i < batchs.Length; i++)
            {
                batchs[i] = new object?[reader.FieldCount];
            }
            var pos = 0;
            var eff = 0;
            while (reader.Read())
            {
                var posItem = batchs[pos++];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var item = reader[i];
                    if (item == DBNull.Value)
                    {
                        item = null;
                    }
                    posItem[i] = item;
                }
                if (pos >= batchSize)
                {
                    eff += await InsertAsync(tableName, columnNames, batchs, token);
                    pos = 0;
                }
            }
            if (pos != 0)
            {
                eff += await InsertAsync(tableName, columnNames, batchs.Take(pos), token);
            }
            return eff;
        }
        public virtual async Task<int> InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<IEnumerable<object?>> values, CancellationToken token = default)
        {
            using (var trans = Connection.BeginTransaction())
            {
                var batchSize = 10;
                var count = 0;
                var header = $"INSERT INTO {SqlType.Wrap(tableName)}({string.Join(",", columnNames.Select(x => SqlType.Wrap(x)))}) VALUES";
                using (var cur = values.GetEnumerator())
                {
                    var sb = new StringBuilder(header);
                    while (cur.MoveNext())
                    {
                        sb.Append($"({string.Join(",", cur.Current.Select(x => SqlType.WrapValue(x)))})");
                        count++;
                        if (count >= batchSize && count > 0)
                        {
                            await ScriptExecuter.ExecuteAsync(sb.ToString(), token: token);
                            sb = new StringBuilder(header);
                            count = 0;
                        }
                        else
                        {
                            sb.Append(',');
                        }
                    }
                    if (count > 0)
                    {
                        await ScriptExecuter.ExecuteAsync(sb.ToString(), token: token);
                        token.ThrowIfCancellationRequested();
                    }
                }
                trans.Commit();
                return count;
            }
        }
        public virtual async Task InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<object?> values, CancellationToken token = default)
        {
            var sql = $"INSERT INTO {SqlType.Wrap(tableName)}({string.Join(",", columnNames.Select(x => SqlType.Wrap(x)))}) VALUES({string.Join(",", values.Select(x => SqlType.WrapValue(x)))})";
            await ScriptExecuter.ExecuteAsync(sql, token: token);
        }

        public void Dispose()
        {
            ScriptExecuter.Dispose();
        }
    }
}
