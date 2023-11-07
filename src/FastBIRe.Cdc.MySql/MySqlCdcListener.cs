using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.Events;
using FastBIRe.Cdc.MySql.Checkpoints;
using MySqlCdc;
using MySqlCdc.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.MySql
{
    public class MySqlCdcListener : CdcListenerBase
    {
        private Task? task;
        private readonly MySqlGetCdcListenerOptions options;
        private readonly Dictionary<long, ITableMapInfo> tableMapInfos = new Dictionary<long, ITableMapInfo>();

        public MySqlCdcListener(BinlogClient binlogClient, MySqlGetCdcListenerOptions options, MySqlCdcModes mode)
            : base(options)
        {
            BinlogClient = binlogClient;
            this.options = options;
            Mode = mode;
        }

        public BinlogClient BinlogClient { get; }

        public MySqlCdcModes Mode { get; }

        public Task? Task => task;

        protected override Task OnStartAsync(CancellationToken token = default)
        {
            task = Task.Factory.StartNew(Handler, this, token, TaskCreationOptions.LongRunning, TaskScheduler.Current)
                .Unwrap();
            return Task.CompletedTask;
        }

        protected override Task OnStopAsync(CancellationToken token = default)
        {
            tableMapInfos.Clear();
            task = null;
            return Task.CompletedTask;
        }
        public ITableMapInfo? GetTableMapInfo(long id)
        {
            return tableMapInfos.TryGetValue(id, out var ifo) ? ifo : null;
        }
        public override ITableMapInfo? GetTableMapInfo(object id)
        {
            if (id is long l)
            {
                return GetTableMapInfo(l);
            }
            return null;
        }

        private async Task Handler(object? state)
        {
            var listener = (MySqlCdcListener)state!;
            var source = listener.TokenSource;
            IGtid? gtid = null;
            long flags = 0;
            await foreach (var item in BinlogClient.Replicate(source!.Token))
            {
                ICheckpoint checkpoint;
                if (Mode == MySqlCdcModes.Gtid)
                {
                    if (item is GtidEvent gtidEvent)
                    {
                        gtid = gtidEvent.Gtid;
                        flags = gtidEvent.Header.ServerId;
                    }
                    checkpoint = new MySqlCheckpoint(gtid, flags);
                }
                else
                {
                    checkpoint = new MySqlCheckpoint(item.Header.NextEventPosition, null);
                }
                if (item is WriteRowsEvent wre)
                {
                    var rows = wre.Rows.Select(x => (ICdcDataRow)new CdcDataRow(x.Cells)).ToList();
                    var insertArg = new InsertEventArgs(wre, wre.TableId, GetTableMapInfo(wre.TableId), rows, checkpoint);
                    RaiseEvent(insertArg);
                }
                else if (item is UpdateRowsEvent ure)
                {
                    var ups = ure.Rows.Select(x => (ICdcUpdateRow)new CdcUpdateRow(
                        new CdcDataRow(x.BeforeUpdate.Cells),
                        new CdcDataRow(x.AfterUpdate.Cells)))
                        .ToList();
                    var up = new UpdateEventArgs(ure, ure.TableId, GetTableMapInfo(ure.TableId), ups, checkpoint);
                    RaiseEvent(up);
                }
                else if (item is DeleteRowsEvent dre)
                {
                    var rows = dre.Rows.Select(x => (ICdcDataRow)new CdcDataRow(x.Cells)).ToList();
                    var insertArg = new DeleteEventArgs(dre, dre.TableId, GetTableMapInfo(dre.TableId), rows, checkpoint);
                    RaiseEvent(insertArg);
                }
                else if (item is TableMapEvent tme)
                {
                    var mapInfo = new TableMapInfo(tme.TableId, tme.DatabaseName, tme.TableName);
                    tableMapInfos[tme.TableId] = mapInfo;
                    RaiseEvent(new TableMapEventArgs(item, tme.TableId, mapInfo, checkpoint));
                }
                else
                {
                    RaiseEvent(new CdcEventArgs(item, checkpoint));
                }
            }
        }
    }
}
