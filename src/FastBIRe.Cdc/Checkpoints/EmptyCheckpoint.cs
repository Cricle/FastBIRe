using System;

namespace FastBIRe.Cdc.Checkpoints
{
    public class EmptyCheckpoint : ICheckpoint
    {
        public static readonly EmptyCheckpoint Instance = new EmptyCheckpoint();

        private EmptyCheckpoint()
        {
        }

        public bool IsEmpty =>true;

        public byte[] ToBytes()
        {
            return Array.Empty<byte>();
        }
    }
}
