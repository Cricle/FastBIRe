﻿using DatabaseSchemaReader;
using FastBIRe.Cdc.Events;
using FastBIRe.Cdc.Mssql.Checkpoints;
using Microsoft.Data.SqlClient;
using System.Numerics;

namespace FastBIRe.Cdc.Mssql
{
    public class MssqlCdcListener : CdcListenerBase
    {
        private Task? task;
        private readonly Dictionary<string, MssqlTableMapInfo> tableMapInfos = new Dictionary<string, MssqlTableMapInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MssqlLsn> maxLsns = new Dictionary<string, MssqlLsn>();
        public MssqlCdcListener(MssqlCdcManager cdcManager, MssqlGetCdcListenerOptions options)
            : base(options)
        {
            CdcManager = cdcManager;
            ScriptExecuter = options.ScriptExecuter;
            DelayScan = options.DelayScan;
            SqlConnection = (SqlConnection)options.ScriptExecuter.Connection;
            DatabaseReader = new DatabaseReader(SqlConnection) { Owner = SqlConnection.Database };
        }
        private IList<string>? cdcTables;
        private MssqlLsn? maxLsn;

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
            task = Task.Factory.StartNew(Handler, this, token, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent, TaskScheduler.Current)
                .Unwrap();
        }

        protected override Task OnStopAsync(CancellationToken token = default)
        {
            tableMapInfos.Clear();
            task = null;
            return Task.CompletedTask;
        }
        protected virtual async Task ReadEventAsync(string lsnStr, IList<CdcEventArgs> raiseList, MssqlReadEventOptions options, CancellationToken token = default)
        {
            var listener = options.Listener;
            var table = options.Table;
            var sql = $"SELECT [__$seqval] AS [seqval],[__$operation] AS [op],{table.ColumnNameJoined} FROM [cdc].[dbo_{table.TableName}_CT] WITH (NOLOCK) WHERE [__$seqval]>{lsnStr}";
            await listener.ScriptExecuter.ReadAsync(sql, (s, r) =>
            {
                while (r.Reader.Read())
                {
                    var seq = new MssqlLsn((byte[])r.Reader[0]);
                    if (seq.Lsn == null || seq > options.Lsn)
                    {
                        options.Lsn = seq;
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
                    var checkpoint = new MssqlCheckpoint(seq);
                    switch (op)
                    {
                        case SqlServerOperator.Delete:
                            raiseList.Add(new DeleteEventArgs(null, table.TableName, table, new ICdcDataRow[] { row },checkpoint));
                            break;
                        case SqlServerOperator.Insert:
                            raiseList.Add(new InsertEventArgs(null, table.TableName, table, new ICdcDataRow[] { row }, checkpoint));
                            break;
                        case SqlServerOperator.AfterUpdate:
                            raiseList.Add(new UpdateEventArgs(null, table.TableName, table, new ICdcUpdateRow[] { new CdcUpdateRow(null, row) }, checkpoint));
                            break;
                        case SqlServerOperator.BeforeUpdate:
                        case SqlServerOperator.Unknow:
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
            var listener = (MssqlCdcListener)state!;
            var source = listener.TokenSource!;
            var tables = listener.cdcTables!;
            var lsn = listener.maxLsn!.Value;
            while (!source.IsCancellationRequested)
            {
                foreach (var item in tables)
                {
                    var table = (MssqlTableMapInfo)GetTableMapInfo(item)!;
                    var raiseList = new List<CdcEventArgs>();
                    try
                    {
                        var opt = new MssqlReadEventOptions(listener, table)
                        {
                            Lsn = lsn
                        };
                        await ReadEventAsync(lsn.LsnString, raiseList, opt, source.Token);
                        lsn = opt.Lsn;
                    }
                    catch (Exception ex)
                        when (ex is not ObjectDisposedException)
                    {
                        RaiseError(new CdcErrorEventArgs(ex));
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
