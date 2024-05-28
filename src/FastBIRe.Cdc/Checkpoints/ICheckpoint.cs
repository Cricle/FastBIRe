using System;

namespace FastBIRe.Cdc.Checkpoints
{
    public static class CheckpointAsExtensions
    {
        public static string ToBase64(this ICheckpoint checkpoint)
        {
            return Convert.ToBase64String(checkpoint.ToBytes());
        }
    }
    public interface ICheckpoint
    {
        bool IsEmpty { get; }

        byte[] ToBytes();
    }
}
