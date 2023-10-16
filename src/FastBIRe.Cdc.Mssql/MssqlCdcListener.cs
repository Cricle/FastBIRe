using DatabaseSchemaReader;
using FastBIRe.Cdc.Events;
using Microsoft.Data.SqlClient;
using System.Numerics;

namespace FastBIRe.Cdc.Mssql
{
    public class MssqlCdcListener : CdcListenerBase
    {
        private Task? task;
        private readonly Dictionary<string, MssqlTableMapInfo> tableMapInfos = new Dictionary<string, MssqlTableMapInfo>(StringComparer.OrdinalIgnoreCase);

        public MssqlCdcListener(MssqlCdcManager cdcManager, IDbScriptExecuter scriptExecuter, TimeSpan delayScan)
        {
            CdcManager = cdcManager;
            ScriptExecuter = scriptExecuter;
            DelayScan = delayScan;
            SqlConnection = (SqlConnection)scriptExecuter.Connection;
            DatabaseReader = new DatabaseReader(SqlConnection) { Owner = SqlConnection.Database };
        }
        private IList<string>? cdcTables;
        private byte[]? maxLsn;

        public MssqlCdcManager CdcManager { get; }

        public IDbScriptExecuter ScriptExecuter { get; }

        public SqlConnection SqlConnection { get; }

        public DatabaseReader DatabaseReader { get; }

        public TimeSpan DelayScan { get; }

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
                tableInfo = new MssqlTableMapInfo(idStr, DatabaseReader.Owner, idStr, table);
                tableMapInfos[idStr] = tableInfo;
            }
            return tableInfo;
        }

        protected override async Task OnStartAsync(CancellationToken token = default)
        {
            maxLsn = await CdcManager.GetMaxLSNAsync(token);
            cdcTables = await CdcManager.GetEnableCdcTableNamesAsync(token);
            task = Task.Factory.StartNew(Handler, this, token, TaskCreationOptions.LongRunning| TaskCreationOptions.AttachedToParent, TaskScheduler.Current)
                .Unwrap();
        }

        protected override Task OnStopAsync(CancellationToken token = default)
        {
            tableMapInfos.Clear();
            task = null;
            return Task.CompletedTask;
        }
        private async Task Handler(object? state)
        {
            var listener = (MssqlCdcListener)state!;
            var source = listener.TokenSource!;
            var tables = listener.cdcTables!;
            var lsn = listener.maxLsn!;
            var lsnBigInt = LsnHelper.LsnToBitInteger(lsn);
            while (!source.IsCancellationRequested)
            {
                var lastLsn = lsn;
                foreach (var item in tables)
                {
                    var table = (MssqlTableMapInfo)GetTableMapInfo(item)!;
                    var lsnStr = LsnHelper.LsnToString(lsn);
                    var raiseList = new List<CdcEventArgs>();
                    try
                    {
                        await listener.ScriptExecuter.ReadAsync($"SELECT [__$seqval] AS [seqval],[__$operation] AS [op],{table.ColumnNameJoined} FROM [cdc].[dbo_{item}_CT] WHERE [__$seqval]>{lsnStr}", (s, r) =>
                        {
                            while (r.Reader.Read())
                            {
                                var seq = (byte[])r.Reader[0];
                                var seqInteger = LsnHelper.LsnToBitInteger(seq);
                                if (seqInteger > lsnBigInt)
                                {
                                    lsn = seq;
                                    lsnBigInt = seqInteger;
                                }
                                var op = (SqlServerOperator)r.Reader.GetInt32(1);
                                if (op == SqlServerOperator.BeforeUpdate)
                                {
                                    continue;
                                }
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
                                switch (op)
                                {
                                    case SqlServerOperator.Delete:
                                        raiseList.Add(new DeleteEventArgs(null, item, table, new ICdcDataRow[] { row }));
                                        break;
                                    case SqlServerOperator.Insert:
                                        raiseList.Add(new InsertEventArgs(null, item, table, new ICdcDataRow[] { row }));
                                        break;
                                    case SqlServerOperator.AfterUpdate:
                                        raiseList.Add(new UpdateEventArgs(null, item, table, new ICdcUpdateRow[] { new CdcUpdateRow(null, row) }));
                                        break;
                                    case SqlServerOperator.BeforeUpdate:
                                    case SqlServerOperator.Unknow:
                                    default:
                                        break;
                                }
                            }
                            return Task.CompletedTask;
                        }, token: source.Token);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    foreach (var raiseItem in raiseList)
                    {
                        RaiseEvent(raiseItem);
                    }
                }
                await Task.Delay(DelayScan);
            }
        }
    }
}
