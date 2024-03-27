namespace FastBIRe.Cdc.Checkpoints
{
    public class EmptyCheckpointManager : ICheckPointManager
    {
        public static readonly EmptyCheckpointManager Instance = new EmptyCheckpointManager();

        private EmptyCheckpointManager()
        {
        }

        public ICheckpoint CreateCheckpoint(byte[] data)
        {
            return EmptyCheckpoint.Instance;
        }
    }
}
