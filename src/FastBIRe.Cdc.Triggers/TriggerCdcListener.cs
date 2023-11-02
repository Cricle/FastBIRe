using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.Cdc.Events;
using FastBIRe.Cdc.Triggers;
using FastBIRe.Cdc.Triggers.Checkpoints;
using FastBIRe.Triggering;

namespace FastBIRe.Cdc.Mssql
{
    public class TriggerCdcListener : CdcListenerBase
    {
        private Task? task;
        private readonly Dictionary<string, TriggerTableMapInfo> tableMapInfos = new Dictionary<string, TriggerTableMapInfo>(StringComparer.OrdinalIgnoreCase);
        public TriggerCdcListener(TriggerGetCdcListenerOptions options)
            : base(options)
        {
            Options = options;
            ScriptExecuter = options.ScriptExecuter;
            DatabaseReader = new DatabaseReader(ScriptExecuter.Connection) { Owner = ScriptExecuter.Connection.Database };
            SqlType = DatabaseReader.SqlType!.Value;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public DatabaseReader DatabaseReader { get; }

        public new TriggerGetCdcListenerOptions Options { get; }

        public SqlType SqlType { get; }

        public override ITableMapInfo? GetTableMapInfo(object id)
        {
            var idStr = id?.ToString();
            if (string.IsNullOrWhiteSpace(idStr))
            {
                return null;
            }
            if (!tableMapInfos.TryGetValue(idStr, out var tableInfo))
            {
                var table = DatabaseReader.Table(idStr);
                if (table == null)
                {
                    return null;
                }
                tableInfo = new TriggerTableMapInfo(idStr, DatabaseReader.Owner, idStr, table, SqlType);
                tableMapInfos[idStr] = tableInfo;
            }
            return tableInfo;
        }

        protected override Task OnStartAsync(CancellationToken token = default)
        {
            task = Task.Factory.StartNew(Handler, this, token, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent, TaskScheduler.Current)
                .Unwrap();
            return Task.CompletedTask;
        }

        protected override Task OnStopAsync(CancellationToken token = default)
        {
            tableMapInfos.Clear();
            task = null;
            return Task.CompletedTask;
        }
        protected virtual async Task ReadEventAsync(IList<CdcEventArgs> raiseList, IList<byte[]> ids, TriggerReadEventOptions options, CancellationToken token = default)
        {
            var listener = options.Listener;
            var table = options.Table;
            var sqlType = options.SqlType;
            var tableHelper = sqlType.GetTableHelper()!;
            var limit = tableHelper.Pagging(null, options.BatchSize);
            var sql = $"SELECT {sqlType.Wrap(TriggerCdcManager.IdColumn)},{sqlType.Wrap(TriggerCdcManager.ActionsColumn)},{table.ColumnNameJoined} FROM {table.TableName} WHERE {sqlType.Wrap(TriggerCdcManager.OkColumn)} = {sqlType.WrapValue(false)} ORDER BY {sqlType.Wrap(TriggerCdcManager.TimeColumn)} {limit}";
            await listener.ScriptExecuter.ReadAsync(sql, (s, r) =>
            {
                while (r.Reader.Read())
                {
                    var id = r.Reader[0];
                    byte[] idBytes;
                    if (id is Guid gid)
                    {
                        idBytes = gid.ToByteArray();
                    }
                    else
                    {
                        idBytes = (byte[])id;
                    }
                    ids.Add(idBytes);
                    var op = (TriggerTypes)r.Reader.GetByte(1);
                    var row = new CdcDataRow();
                    for (int i = 2; i < r.Reader.FieldCount; i++)
                    {
                        var val = r.Reader[i];
                        if (val == DBNull.Value)
                        {
                            val = null;
                        }
                        row.Add(val);
                    }
                    var checkpoint = new TriggerCheckpoint(idBytes);
                    switch (op)
                    {
                        case TriggerTypes.AfterDelete:
                            raiseList.Add(new DeleteEventArgs(null, table.TableName, table, new ICdcDataRow[] { row }, checkpoint));
                            break;
                        case TriggerTypes.AfterInsert:
                            raiseList.Add(new InsertEventArgs(null, table.TableName, table, new ICdcDataRow[] { row }, checkpoint));
                            break;
                        case TriggerTypes.AfterUpdate:
                            raiseList.Add(new UpdateEventArgs(null, table.TableName, table, new ICdcUpdateRow[] { new CdcUpdateRow(null, row) }, checkpoint));
                            break;
                        default:
                            raiseList.Add(new CdcEventArgs(row, checkpoint));
                            break;
                    }
                }
                return Task.CompletedTask;
            }, token: token);
        }

        private async Task Handler(object? state)
        {
            var listener = (TriggerCdcListener)state!;
            var source = listener.TokenSource!;
            var sqlType = listener.DatabaseReader.SqlType!.Value;
            var batchSize = (int)listener.Options.ReadBatch;
            var tables = listener.Options.TableNames!;
            var scriptExecuter = listener.Options.ScriptExecuter;
            var dbc = scriptExecuter.Connection;
            var delay = listener.Options.DelayScan;
            while (!source.IsCancellationRequested)
            {
                foreach (var item in tables)
                {
                    var table = (TriggerTableMapInfo)GetTableMapInfo(item)!;
                    var raiseList = new List<CdcEventArgs>();
                    var ids = new List<byte[]>();
                    try
                    {
                        var opt = new TriggerReadEventOptions(listener, table, sqlType, batchSize);
                        await ReadEventAsync(raiseList, ids, opt, source.Token);
                        if (ids.Count != 0)
                        {
                            foreach (var raiseItem in raiseList)
                            {
                                RaiseEvent(raiseItem);
                            }
                            var idFilter = string.Join(",", ids.Select(x => sqlType.WrapValue(x)));
                            var updates = $"UPDATE {sqlType.Wrap(item)} SET {sqlType.Wrap(TriggerCdcManager.OkColumn)} = {sqlType.WrapValue(true)} WHERE {sqlType.Wrap(TriggerCdcManager.IdColumn)} IN ({idFilter})";
                            await scriptExecuter.ExecuteAsync(updates, token: source.Token);
                        }
                    }
                    catch (Exception ex)
                        when (ex is not ObjectDisposedException)
                    {
                        RaiseError(new CdcErrorEventArgs(ex));
                    }
                }
                await Task.Delay(delay);
            }
        }
    }
}
