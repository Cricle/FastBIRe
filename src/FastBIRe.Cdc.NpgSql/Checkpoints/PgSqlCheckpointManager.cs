using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.NpgSql.Checkpoints
{
    public class PgSqlCheckpointManager : ICheckPointManager
    {
        public static readonly PgSqlCheckpointManager Instance = new PgSqlCheckpointManager();

        public ICheckpoint CreateCheckpoint(byte[] data)
        {
            return PgSqlCheckpoint.FromBytes(data);
        }
    }
}
