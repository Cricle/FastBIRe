using FastBIRe.Cdc.Checkpoints;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using NpgsqlTypes;

namespace FastBIRe.Cdc.NpgSql
{
    public class PgSqlGetCdcListenerOptions : GetCdcListenerOptions
    {
        public PgSqlGetCdcListenerOptions(LogicalReplicationConnection logicalReplicationConnection,
            PgOutputReplicationSlot outputReplicationSlot,
            PgOutputReplicationOptions outputReplicationOptions,
            NpgsqlLogSequenceNumber? npgsqlLogSequenceNumber,
            ICheckpoint? checkpoint)
            : base( checkpoint)
        {
            LogicalReplicationConnection = logicalReplicationConnection;
            OutputReplicationSlot = outputReplicationSlot;
            OutputReplicationOptions = outputReplicationOptions;
            NpgsqlLogSequenceNumber = npgsqlLogSequenceNumber;
        }

        public LogicalReplicationConnection LogicalReplicationConnection { get; }

        public PgOutputReplicationSlot OutputReplicationSlot { get; }

        public PgOutputReplicationOptions OutputReplicationOptions { get; }

        public NpgsqlLogSequenceNumber? NpgsqlLogSequenceNumber { get; }

        public static PgSqlGetCdcListenerOptions CreateDefault(LogicalReplicationConnection connection,
            string databaseName,
            string tableName,
            NpgsqlLogSequenceNumber? npgsqlLogSequenceNumber = null,
            ICheckpoint? checkpoint = null)
        {
            var slotName = PgSqlCdcManager.GetSlotName(databaseName, tableName);
            var pubName = PgSqlCdcManager.GetPubName(databaseName, tableName);
            return new PgSqlGetCdcListenerOptions(connection,
                new PgOutputReplicationSlot(slotName!),
                new PgOutputReplicationOptions(pubName!, 1),
                npgsqlLogSequenceNumber, checkpoint);
        }
    }
}
