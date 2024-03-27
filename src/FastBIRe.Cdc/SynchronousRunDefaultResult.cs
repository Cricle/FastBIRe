using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.Events;

namespace FastBIRe.Cdc
{
    public readonly struct SynchronousRunDefaultResult
    {
        public SynchronousRunDefaultResult(ICheckpoint? checkpoint, IEventDispatcher<CdcEventArgs> eventDispatcher, ICdcListener listener)
        {
            Checkpoint = checkpoint;
            EventDispatcher = eventDispatcher;
            Listener = listener;
        }

        public ICheckpoint? Checkpoint { get; }

        public IEventDispatcher<CdcEventArgs> EventDispatcher { get; }

        public ICdcListener Listener { get; }
    }
}
