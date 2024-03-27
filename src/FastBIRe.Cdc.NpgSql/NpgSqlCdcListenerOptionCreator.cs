using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using NpgsqlTypes;

namespace FastBIRe.Cdc.NpgSql
{
    public class NpgSqlCdcListenerOptionCreator : ICdcListenerOptionCreator
    {
        public NpgSqlCdcListenerOptionCreator(string connectionString)
            :this(new LogicalReplicationConnection(connectionString))
        {

        }
        public NpgSqlCdcListenerOptionCreator(LogicalReplicationConnection logicalReplicationConnection)
        {
            LogicalReplicationConnection = logicalReplicationConnection ?? throw new ArgumentNullException(nameof(logicalReplicationConnection));
        }

        public LogicalReplicationConnection LogicalReplicationConnection { get; }

        public PgOutputReplicationSlot? OutputReplicationSlot { get; }

        public PgOutputReplicationOptions? OutputReplicationOptions { get; }

        public NpgsqlLogSequenceNumber? NpgsqlLogSequenceNumber { get; }

        public async Task<ICdcListener> CreateCdcListnerAsync(CdcListenerOptionCreateInfo info, CancellationToken token = default)
        {
            var sourceDbName = info.Runner.SourceConnection.Database;
            var sourceTableName = info.Runner.SourceTableName;
            var slot = OutputReplicationSlot;
            if (slot == null)
            {
                var name = PgSqlCdcManager.GetSlotName(sourceDbName, sourceTableName);
                slot = new PgOutputReplicationSlot(new ReplicationSlotOptions(name!));
            }
            var options = OutputReplicationOptions;
            if (options == null)
            {
                var name = PgSqlCdcManager.GetPubName(sourceDbName, sourceTableName);
                options = new PgOutputReplicationOptions(name!, 1);
            }
            try
            {
                //Check the connection is open?
                _ = LogicalReplicationConnection.ProcessID;
            }
            catch (Exception)
            {
                await LogicalReplicationConnection.Open(token);
            }
            return await info.Runner.CdcManager.GetCdcListenerAsync(new PgSqlGetCdcListenerOptions(
                LogicalReplicationConnection,
                slot,
                options,
                NpgsqlLogSequenceNumber,
                info.CheckPoint),
                token);
        }
    }
}
