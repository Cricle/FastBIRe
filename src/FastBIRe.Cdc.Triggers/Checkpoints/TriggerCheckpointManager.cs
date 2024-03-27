using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.Triggers.Checkpoints
{
    public class TriggerCheckpointManager : ICheckPointManager
    {
        public static readonly TriggerCheckpointManager Instance = new TriggerCheckpointManager();

        private TriggerCheckpointManager() { }

        public ICheckpoint CreateCheckpoint(byte[] data)
        {
            return new TriggerCheckpoint(data);
        }
    }
}
