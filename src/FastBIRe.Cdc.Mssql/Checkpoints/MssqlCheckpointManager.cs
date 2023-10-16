using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.Mssql.Checkpoints
{
    public class MssqlCheckpointManager : ICheckPointManager
    {
        public static readonly MssqlCheckpointManager Instance = new MssqlCheckpointManager();

        public ICheckpoint CreateCheckpoint(byte[] data)
        {
            return new MssqlCheckpoint(data);
        }
    }
}
