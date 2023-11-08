using FastBIRe.Cdc.Checkpoints;
using System.Collections.Generic;

namespace FastBIRe.Cdc
{
    public abstract class GetCdcListenerOptions : IGetCdcListenerOptions
    {
        protected GetCdcListenerOptions(IReadOnlyList<string>? tableNames, ICheckpoint? checkpoint)
        {
            TableNames = tableNames;
            Checkpoint = checkpoint;
        }

        public IReadOnlyList<string>? TableNames { get; }

        public ICheckpoint? Checkpoint { get; }
    }
}
