using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc
{
    public abstract class GetCdcListenerOptions : IGetCdcListenerOptions
    {
        protected GetCdcListenerOptions(ICheckpoint? checkpoint)
        {
            Checkpoint = checkpoint;
        }

        public ICheckpoint? Checkpoint { get; }
    }
}
