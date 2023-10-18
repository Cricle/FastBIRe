using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using NpgsqlTypes;

namespace FastBIRe.Cdc.NpgSql
{
    public class PgSqlGetCdcListenerOptions : IGetCdcListenerOptions
    {
        public PgSqlGetCdcListenerOptions(LogicalReplicationConnection logicalReplicationConnection,
            PgOutputReplicationSlot outputReplicationSlot,
            PgOutputReplicationOptions outputReplicationOptions,
            NpgsqlLogSequenceNumber? npgsqlLogSequenceNumber, IReadOnlyList<string>? tableNames)
        {
            TableNames = tableNames;
            LogicalReplicationConnection = logicalReplicationConnection;
            OutputReplicationSlot = outputReplicationSlot;
            OutputReplicationOptions = outputReplicationOptions;
            NpgsqlLogSequenceNumber = npgsqlLogSequenceNumber;
        }

        public IReadOnlyList<string>? TableNames { get; }

        public LogicalReplicationConnection LogicalReplicationConnection { get; }

        public PgOutputReplicationSlot OutputReplicationSlot { get; }

        public PgOutputReplicationOptions OutputReplicationOptions { get; }

        public NpgsqlLogSequenceNumber? NpgsqlLogSequenceNumber { get; }
    }
}
