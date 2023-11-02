using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.Triggers.Checkpoints
{
    public readonly struct TriggerCheckpoint : ICheckpoint
    {
        public TriggerCheckpoint(byte[] bytes)
        {
            Bytes = bytes;
        }

        public byte[] Bytes { get; }

        public byte[] ToBytes()
        {
            return Bytes;
        }
    }
}
