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
            IReadOnlyList<string>? tableNames,
            ICheckpoint? checkpoint)
            :base(tableNames, checkpoint) 
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
    }
}
