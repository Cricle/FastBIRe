using System;

namespace FastBIRe.Cdc.Checkpoints
{
    public class CheckpointPackage
    {
        public CheckpointPackage(CheckpointIdentity identity, byte[]? checkpointData)
        {
            Identity = identity;
            CheckpointData = checkpointData;
        }

        public CheckpointIdentity Identity { get; }

        public byte[]? CheckpointData { get; }

        public TCheckpoint? CastCheckpoint<TCheckpoint>(ICheckPointManager mgr)
            where TCheckpoint:ICheckpoint
        {
            if (TryCastCheckpoint(mgr,out var cp,out var ex))
            {
                if (cp is TCheckpoint tcp)
                {
                    return tcp;
                }
                throw new InvalidCastException($"Can't case {cp?.GetType()} to {typeof(TCheckpoint)}");
            }
            if (ex != null)
            {
                throw ex;
            }
            return default;
        }
        public bool TryCastCheckpoint(ICheckPointManager mgr, out ICheckpoint? checkpoint, out Exception? exception)
        {
            checkpoint = null;
            exception = null;

            try
            {
                if (CheckpointData == null)
                {
                    return false;
                }
                checkpoint = mgr.CreateCheckpoint(CheckpointData);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }
    }
}
