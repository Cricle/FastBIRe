using FastBIRe.Cdc.Events;
using FastBIRe.Cdc.NpgSql.Checkpoints;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;
using NpgsqlTypes;

namespace FastBIRe.Cdc.NpgSql
{
    public class PgSqlCdcListener : CdcListenerBase
    {
        private Task? task;
        private readonly Dictionary<uint, ITableMapInfo> tableMapInfos = new Dictionary<uint, ITableMapInfo>();

        public PgSqlCdcListener(PgSqlGetCdcListenerOptions options)
            : base(options)
        {
            ReplicationConnectionConnection = options.LogicalReplicationConnection;
            OutputReplicationSlot = options.OutputReplicationSlot;
            OutputReplicationOptions = options.OutputReplicationOptions;
            NpgsqlLogSequenceNumber = options.NpgsqlLogSequenceNumber;
            if (options.Checkpoint is PgSqlCheckpoint cp && cp.SequenceNumber != null)
            {
                ReplicationConnectionConnection.SetReplicationStatus(cp.SequenceNumber.Value);
            }
        }

        public LogicalReplicationConnection ReplicationConnectionConnection { get; }

        public PgOutputReplicationSlot OutputReplicationSlot { get; }

        public PgOutputReplicationOptions OutputReplicationOptions { get; }

        public NpgsqlLogSequenceNumber? NpgsqlLogSequenceNumber { get; }

        public ITableMapInfo? GetTableMapInfo(uint id)
        {
            return tableMapInfos.TryGetValue(id, out var ifo) ? ifo : null;
        }
        public override ITableMapInfo? GetTableMapInfo(object id)
        {
            if (id is uint l)
            {
                return GetTableMapInfo(l);
            }
            return null;
        }

        protected override Task OnStartAsync(CancellationToken token = default)
        {
            task = Task.Factory.StartNew(Handler, this, token, TaskCreationOptions.LongRunning, TaskScheduler.Current)
                .Unwrap();
            return Task.CompletedTask;
        }

        protected override async Task OnStopAsync(CancellationToken token = default)
        {
            tableMapInfos.Clear();
            if (task != null)
            {
                await task;
            }
            task = null;
        }
        private static async Task<IList<object?>> ReadRowAsync(ReplicationTuple tuple, CancellationToken token = default)
        {
            var res = new List<object?>();
            await foreach (var item in tuple)
            {
                var obj = await item.Get(token);
                if (obj == DBNull.Value)
                {
                    res.Add(null);
                }
                else
                {
                    res.Add(obj);
                }
            }
            return res;
        }
        private async Task Handler(object? state)
        {
            var listener = (PgSqlCdcListener)state!;
            var source = listener.TokenSource;
            var connection = listener.ReplicationConnectionConnection;
            await foreach (var message in connection.StartReplication(
                listener.OutputReplicationSlot, OutputReplicationOptions, source!.Token, NpgsqlLogSequenceNumber))
            {
                var checkpoint = new PgSqlCheckpoint(message.WalEnd);
                if (message is FullUpdateMessage fum)
                {
                    var old = await ReadRowAsync(fum.OldRow);
                    var @new = await ReadRowAsync(fum.NewRow);
                    var ev = new UpdateEventArgs(fum, fum.Relation.RelationId, GetTableMapInfo(fum.Relation.RelationId), new ICdcUpdateRow[]
                    {
                        new CdcUpdateRow(new CdcDataRow(old),new CdcDataRow(@new))
                    }, checkpoint);
                    RaiseEvent(ev);
                }
                else if (message is DefaultUpdateMessage dum)
                {
                    var @new = await ReadRowAsync(dum.NewRow);
                    var ev = new UpdateEventArgs(dum, dum.Relation.RelationId, GetTableMapInfo(dum.Relation.RelationId), new ICdcUpdateRow[]
                    {
                        new CdcUpdateRow(null,new CdcDataRow(@new))
                    }, checkpoint);
                    RaiseEvent(ev);
                }
                else if (message is FullDeleteMessage fdm)
                {
                    var rowData = await ReadRowAsync(fdm.OldRow);
                    var ev = new DeleteEventArgs(fdm, fdm.Relation.RelationId, GetTableMapInfo(fdm.Relation.RelationId), new ICdcDataRow[]
                    {
                        new CdcDataRow(rowData)
                    }, checkpoint);
                    RaiseEvent(ev);
                }
                else if (message is KeyDeleteMessage kdm)
                {
                    var rowData = await ReadRowAsync(kdm.Key);
                    var ev = new DeleteEventArgs(kdm, kdm.Relation.RelationId, GetTableMapInfo(kdm.Relation.RelationId), new ICdcDataRow[]
                    {
                        new CdcDataRow(rowData)
                    }, checkpoint);
                    RaiseEvent(ev);
                }
                else if (message is InsertMessage im)
                {
                    var rowData = await ReadRowAsync(im.NewRow);
                    var ev = new InsertEventArgs(im, im.Relation.RelationId, GetTableMapInfo(im.Relation.RelationId), new ICdcDataRow[]
                    {
                        new CdcDataRow(rowData)
                    }, checkpoint);
                    RaiseEvent(ev);
                }
                else if (message is RelationMessage rm)
                {
                    var tbIfo = new TableMapInfo(rm.RelationId, rm.Namespace, rm.RelationName);
                    tableMapInfos[rm.RelationId] = tbIfo;
                    var ev = new TableMapEventArgs(rm, rm.RelationName, tbIfo, checkpoint);
                    RaiseEvent(ev);
                }
                else
                {
                    RaiseEvent(new CdcEventArgs(message, checkpoint));
                }
                connection.SetReplicationStatus(message.WalEnd);
            }
        }
    }
}
