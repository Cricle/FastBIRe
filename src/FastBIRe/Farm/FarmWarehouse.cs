﻿using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using DatabaseSchemaReader.Utilities;
using System.Data;
using System.Data.Common;
using System.Text;

namespace FastBIRe.Farm
{
    public class FarmWarehouse : IDisposable
    {
        public const string CursorTable = "cursor";

        public const string SeqTable = "seq";

        public const string SyncNamer = "name";

        public const string SyncPoint = "point";

        public const string Id = "id";

        public FarmWarehouse(IDbScriptExecuter scriptExecuter, ICursorRowHandlerSelector cursorRowHandlerSelector, bool attackId)
            : this(scriptExecuter, new DatabaseReader(scriptExecuter.Connection) { Owner = scriptExecuter.Connection.Database }, cursorRowHandlerSelector,attackId)
        {
            AttackId = attackId;
        }
        public FarmWarehouse(IDbScriptExecuter scriptExecuter, DatabaseReader databaseReader, ICursorRowHandlerSelector cursorRowHandlerSelector, bool attackId)
        {
            ScriptExecuter = scriptExecuter;
            DatabaseReader = databaseReader;
            SqlType = databaseReader.SqlType!.Value;
            TableHelper = new TableHelper(SqlType);
            CursorRowHandlerSelector = cursorRowHandlerSelector;
            AttackId = attackId;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public DatabaseReader DatabaseReader { get; }

        public DbConnection Connection => ScriptExecuter.Connection;

        public SqlType SqlType { get; }

        public TableHelper TableHelper { get; }

        public ICursorRowHandlerSelector CursorRowHandlerSelector { get; }

        protected virtual DatabaseTable CreateCursorTable()
        {
            var cursorTable = new DatabaseTable { Name = CursorTable, DatabaseSchema = DatabaseReader.DatabaseSchema };
            cursorTable.AddColumn(SyncNamer).SetType(SqlType, DbType.String, 64);//TODO
            cursorTable.AddColumn(SyncPoint, DbType.Int64);
            cursorTable.AddConstraint(new DatabaseConstraint { Name = $"PK_{CursorTable}", Columns = { SyncNamer }, ConstraintType = ConstraintType.PrimaryKey });
            return cursorTable;
        }
        protected virtual DatabaseTable CreateSeqTable()
        {
            var cursorTable = new DatabaseTable { Name = SeqTable, DatabaseSchema = DatabaseReader.DatabaseSchema };
            cursorTable.AddColumn(Id).SetTypeDefault(SqlType, DbType.UInt64);
            return cursorTable;
        }

        public bool AttackId { get; }

        public async Task<IList<string>> GetSyncScriptsAsync(DatabaseTable table, IEnumerable<int>? maskColumns=null, CancellationToken token = default)
        {
            var scripts = new List<string>();
            var copyTable = table.Clone();
            if (maskColumns!=null)
            {
                var removes = copyTable.Columns.Where((x, i) => maskColumns.Contains(i)).ToList();
                copyTable.Columns.RemoveAll(x => !removes.Contains(x));
            }
            copyTable.SchemaOwner = DatabaseReader.Owner;
            copyTable.DatabaseSchema = DatabaseReader.DatabaseSchema;
            if (AttackId)
            {
                copyTable.AddColumn(Id, DbType.UInt64, c => c.Nullable = true);
                var constr = new DatabaseConstraint { Name = $"PK_{table.Name}", Columns = { Id }, ConstraintType = ConstraintType.PrimaryKey };
                copyTable.PrimaryKey = null;
                copyTable.AddConstraint(constr);
            }
            copyTable.Indexes = new List<DatabaseIndex>();
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
                        if (dest.DataType.NetDataType != x.DataType.NetDataType)
                        {
                            return false;
                        }
                        if (dest.DataType.IsString && dest.Length != x.Length)
                        {
                            return false;
                        }
                        if (dest.DataType.IsNumeric && dest.DataType.NetDataType == typeof(decimal).FullName)
                        {
                            return dest.Scale == x.Scale && dest.Precision == x.Precision;
                        }
                        return true;
                    });
                if (structIsEquals)
                {
                    return scripts;
                }
                var hasCurTable = DatabaseReader.TableExists(CursorTable);
                if (hasCurTable&&AttackId)
                {
                    var anyDatas = await AnyCheckpointNotComplatedAsync(table.Name);
                    if (anyDatas)
                    {
                        await CheckPointAsync(table.Name, cursorNames: null, token: token);
                    }
                }
                scripts.Add(TableHelper.CreateDropTable(copyTable.Name));
                if (DatabaseReader.TableExists(CursorTable))
                {
                    scripts.Add(TableHelper.CreateDropTable(CursorTable));
                }
                if (DatabaseReader.TableExists(SeqTable))
                {
                    scripts.Add(TableHelper.CreateDropTable(SeqTable));
                }
            }
            var ddlFactory = new DdlGeneratorFactory(SqlType);
            var ddlTable = ddlFactory.TableGenerator(copyTable).Write();
            var ddlCursor = ddlFactory.TableGenerator(CreateCursorTable()).Write();
            scripts.Add(ddlTable);
            scripts.Add(ddlCursor);
            if (AttackId)
            {
                var ddlSeq = ddlFactory.TableGenerator(CreateSeqTable()).Write();
                scripts.Add(ddlSeq);
            }
            return scripts;
        }
        public async Task SyncAsync(DatabaseTable table, IEnumerable<int>? maskColumns = null, CancellationToken token = default)
        {
            var scripts = await GetSyncScriptsAsync(table, maskColumns,token);
            if (scripts.Count != 0)
            {
                await ScriptExecuter.ExecuteBatchAsync(scripts, token: token);
            }
        }
        public Task SyncDataAsync(string tableName, IDataReader reader, int batchSize, CancellationToken token = default)
        {
            var columnNames = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames[i] = reader.GetName(i);
            }
            return SyncDataAsync(tableName, columnNames, reader, batchSize, token);
        }
        public async Task SyncDataAsync(string tableName, IEnumerable<string> columnNames, IDataReader reader, int batchSize, CancellationToken token = default)
        {
            var batchs = new object?[batchSize][];
            for (int i = 0; i < batchs.Length; i++)
            {
                batchs[i] = new object?[reader.FieldCount];
            }
            var pos = 0;
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
                    await InsertAsync(tableName, columnNames, batchs, token);
                    pos = 0;
                }
            }
            if (pos != 0)
            {
                await InsertAsync(tableName, columnNames, batchs.Take(pos), token);
            }
        }
        public virtual async Task AddIfSeqNothingAsync()
        {
            var sql = TableHelper.InsertUnionValues(SeqTable, new[] { Id }, new object[] { 0 }, $"NOT EXISTS (SELECT 1 FROM {SqlType.Wrap(SeqTable)})");
            await ScriptExecuter.ExecuteAsync(sql);
        }
        public Task<long> GetDataLastIdAsync(string tableName,CancellationToken token=default)
        {
            var maxIdSql = $"SELECT MAX({SqlType.Wrap(Id)}) FROM {SqlType.Wrap(tableName)}";
            return ScriptExecuter.ReadOneAsync<long>(maxIdSql, token: token);
        }
        public virtual async Task<bool> AnyCheckpointNotComplatedAsync(string tableName, CancellationToken token = default)
        {
            var anyDataSql = $"SELECT 1 FROM {SqlType.Wrap(CursorTable)} AS {SqlType.Wrap("ct")} WHERE {SqlType.Wrap("ct")}.{SqlType.Wrap(SyncPoint)} <= (SELECT MAX({SqlType.Wrap("t")}.{SqlType.Wrap(Id)}) FROM {SqlType.Wrap(tableName)} AS {SqlType.Wrap("t")}) {TableHelper.Pagging(null, 1)}";
            return await ScriptExecuter.ExistsAsync(anyDataSql);
        }
        public virtual async Task InsertAsync(string tableName,IEnumerable<string> columnNames,IEnumerable<IEnumerable<object?>> values, CancellationToken token = default)
        {
            using (var trans = Connection.BeginTransaction())
            {
                ulong id = 0;
                if (AttackId)
                {
                    id = await GetCurrentSeqAsync(token);
                }
                var batchSize = 10;
                var header = $"INSERT INTO {SqlType.Wrap(tableName)}({string.Join(",", columnNames.Select(x => SqlType.Wrap(x)))}) VALUES";
                using (var cur = values.GetEnumerator())
                {
                    var sb = new StringBuilder(header);
                    var count = 0;
                    while (cur.MoveNext())
                    {
                        id++;//seq ++
                        var str = $"({string.Join(",", cur.Current.Select(x => SqlType.WrapValue(x)))}";
                        if (AttackId)
                        {
                            sb.Append(str);
                            sb.Append(',');
                            sb.Append(id);
                        }
                            sb.Append(')');
                        count++;
                        if (count >= batchSize && count > 0)
                        {
                            await ScriptExecuter.ExecuteAsync(sb.ToString());
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
                if (AttackId)
                {
                    await UpdateCurrentSeqAsync(id);
                }
                trans.Commit();
            }
        }
        public virtual Task<ulong> GetCurrentSeqAsync(CancellationToken token=default)
        {
            var sql = $"SELECT {SqlType.Wrap(Id)} FROM {SqlType.Wrap(SeqTable)} {TableHelper.Pagging(null, 1)}";
            return ScriptExecuter.ReadOneAsync<ulong>(sql, token: token);
        }
        public virtual Task<int> UpdateCurrentSeqAsync(ulong seq, CancellationToken token = default)
        {
            var sql = $"UPDATE {SqlType.Wrap(SeqTable)} SET {SqlType.Wrap(Id)} = {SqlType.WrapValue(seq)}";
            return ScriptExecuter.ExecuteAsync(sql, token: token);
        }
        public virtual async Task InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<object?> values, CancellationToken token = default)
        {
            using (var trans = Connection.BeginTransaction())
            {
                ulong id = 0;
                if (AttackId)
                {
                    id = await GetCurrentSeqAsync();
                }
                id++;
                var sql = $"INSERT INTO {SqlType.Wrap(tableName)}({string.Join(",", columnNames.Select(x => SqlType.Wrap(x)))}) VALUES({string.Join(",", values.Select(x => SqlType.WrapValue(x)))}";
                if (AttackId)
                {
                    sql += $",{id}";
                }
                sql += ")";
                await ScriptExecuter.ExecuteAsync(sql, token: token);
                if (AttackId)
                {
                    await UpdateCurrentSeqAsync(id);
                }

                trans.Commit();
            }
        }
        public virtual async Task RemoveCheckpointAsync(string name,CancellationToken token=default)
        {
            var sql = $"DELETE FROM {SqlType.Wrap(CursorTable)} WHERE {SqlType.Wrap(SyncNamer)} = {SqlType.WrapValue(name)}";
            await ScriptExecuter.ExecuteAsync(sql, token: token);
        }
        public virtual Task<long?> GetCheckpointAsync(string name, CancellationToken token = default)
        {
            var sql = $"SELECT {SqlType.Wrap(SyncPoint)} FROM {SqlType.Wrap(CursorTable)} WHERE {SqlType.Wrap(SyncNamer)} = {SqlType.WrapValue(name)}";
            return ScriptExecuter.ReadOneAsync<long?>(sql, token: token);
        }
        public virtual Task<long?> UpdateCheckpointAsync(string name,long point, CancellationToken token = default)
        {
            var sql = $"UPDATE {SqlType.Wrap(CursorTable)} SET {SqlType.Wrap(SyncPoint)} = {point} WHERE {SqlType.Wrap(SyncNamer)} = {SqlType.WrapValue(name)}";
            return ScriptExecuter.ReadOneAsync<long?>(sql, token: token);
        }
        public virtual async Task CreateCheckPointIfNotExistsAsync(string name)
        {
            var sql = TableHelper.InsertUnionValues(CursorTable, new[] { SyncNamer,SyncPoint }, new object[] { name,0 }, $"NOT EXISTS (SELECT 1 FROM {SqlType.Wrap(CursorTable)} WHERE {SqlType.Wrap(SyncNamer)} = {SqlType.WrapValue(name)})");
            await ScriptExecuter.ExecuteAsync(sql);
        }
        public async Task<IList<ICursorRowHandlerResult>> CheckPointAsync(string tableName,IEnumerable<string>? cursorNames=null,CancellationToken token = default)
        {
            var querySql = $"SELECT {SqlType.Wrap(SyncNamer)} AS Name,{SqlType.Wrap(SyncPoint)} AS Point FROM {SqlType.Wrap(CursorTable)}";
            if (cursorNames != null && cursorNames.Any())
            {
                querySql += $" WHERE {SqlType.Wrap(SyncNamer)} IN ({string.Join(",", cursorNames.Select(x => SqlType.WrapValue(x)))})";
            }
            var cursors = new List<CursorRow>();
            await ScriptExecuter.ReadAsync(querySql, (s, e) =>
            {
                while (e.Reader.Read())
                {
                    var name = e.Reader.GetString(0);
                    var point = e.Reader.GetInt64(1);
                    cursors.Add(new CursorRow(name, (ulong)point));
                }
                return Task.CompletedTask;
            }, token: token);
            var dataLastId = await GetDataLastIdAsync(tableName, token);
            var results = new List<ICursorRowHandlerResult>();
            foreach (var item in cursors)
            {
                var res = await CursorRowHandlerSelector.GetHandler(item).HandlerCursorRowAsync(item, token);
                results.Add(res);
            }
            return results;
        }

        public void Dispose()
        {
            ScriptExecuter.Dispose();
        }
    }
}