using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc
{
    public readonly record struct CdcListenerOptionCreateInfo
    {
        public readonly SynchronousRunner Runner;

        public readonly ICheckpoint? CheckPoint;

        public CdcListenerOptionCreateInfo(SynchronousRunner runner, ICheckpoint? checkPoint)
        {
            CheckPoint = checkPoint;
            Runner = runner;
        }
    }
}
