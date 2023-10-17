using System;

namespace FastBIRe.Cdc.Checkpoints
{
    public class EmptyCheckpoint : ICheckpoint
    {
        public static readonly EmptyCheckpoint Instance = new EmptyCheckpoint();

        private EmptyCheckpoint()
        {
        }

        public byte[] ToBytes()
        {
            return Array.Empty<byte>();
        }
    }
}
