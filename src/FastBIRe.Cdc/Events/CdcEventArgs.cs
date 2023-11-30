using FastBIRe.Cdc.Checkpoints;
using System;

namespace FastBIRe.Cdc.Events
{
    public class CdcEventArgs : EventArgs
    {
        public CdcEventArgs(object? rawData, ICheckpoint? checkpoint)
        {
            RawData = rawData;
            Checkpoint = checkpoint;
        }

        public object? RawData { get; }

        public ICheckpoint? Checkpoint { get; }

        public bool HasCheckpoint => Checkpoint != null && !Checkpoint.IsEmpty;
    }
}
