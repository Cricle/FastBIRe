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

        public ICheckpoint? CastCheckpoint(ICheckPointManager mgr)
        {
            if (TryCastCheckpoint(mgr,out var cp,out var ex))
            {
                return cp;
            }
            if (ex != null)
            {
                throw ex;
            }
            return null;
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
