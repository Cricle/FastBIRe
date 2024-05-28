namespace FastBIRe.Cdc.Checkpoints
{
    public interface ICheckPointManager
    {
        ICheckpoint CreateCheckpoint(byte[] data);
    }
}
