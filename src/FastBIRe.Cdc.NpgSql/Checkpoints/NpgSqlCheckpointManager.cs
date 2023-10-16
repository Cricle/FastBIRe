using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.NpgSql.Checkpoints
{
    public class NpgSqlCheckpointManager : ICheckPointManager
    {
        public static readonly NpgSqlCheckpointManager Instance = new NpgSqlCheckpointManager();

        public ICheckpoint CreateCheckpoint(byte[] data)
        {
            return NpgSqlCheckpoint.FromBytes(data);
        }
    }
}
