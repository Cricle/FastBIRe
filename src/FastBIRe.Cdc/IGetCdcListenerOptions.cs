using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc
{
    public interface IGetCdcListenerOptions
    {
        ICheckpoint? Checkpoint { get; }
    }
}
