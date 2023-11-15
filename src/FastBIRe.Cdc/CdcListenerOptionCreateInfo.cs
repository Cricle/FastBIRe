using FastBIRe.Cdc.Checkpoints;
using System.Collections.Generic;

namespace FastBIRe.Cdc
{
    public readonly record struct CdcListenerOptionCreateInfo
    {
        public readonly SynchronousRunner Runner;

        public readonly ICheckpoint? CheckPoint;

        public readonly IReadOnlyList<string>? TableNames;

        public CdcListenerOptionCreateInfo(SynchronousRunner runner, ICheckpoint? checkPoint, IReadOnlyList<string>? tableNames)
        {
            CheckPoint = checkPoint;
            TableNames = tableNames;
            Runner = runner;
        }
    }
}
