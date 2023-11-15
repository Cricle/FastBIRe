using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.Mssql
{
    public class TriggerGetCdcListenerOptions : GetCdcListenerOptions
    {
        public TriggerGetCdcListenerOptions(IDbScriptExecuter scriptExecuter, TimeSpan delayScan, uint readBatch, IReadOnlyList<string>? tableNames, ICheckpoint? checkpoint)
            : base(tableNames, checkpoint)
        {
            DelayScan = delayScan;
            ScriptExecuter = scriptExecuter;
            ReadBatch = readBatch;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public TimeSpan DelayScan { get; }

        public uint ReadBatch { get; }
    }
}
