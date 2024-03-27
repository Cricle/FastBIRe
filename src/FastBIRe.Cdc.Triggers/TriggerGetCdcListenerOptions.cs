using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.Mssql
{
    public class TriggerGetCdcListenerOptions : GetCdcListenerOptions
    {
        public TriggerGetCdcListenerOptions(IDbScriptExecuter scriptExecuter, TimeSpan delayScan, uint readBatch, ICheckpoint? checkpoint, IEnumerable<string> tableNames)
            : base(checkpoint)
        {
            DelayScan = delayScan;
            ScriptExecuter = scriptExecuter;
            ReadBatch = readBatch;
            TableNames = tableNames;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public TimeSpan DelayScan { get; }

        public uint ReadBatch { get; }

        public IEnumerable<string> TableNames { get; }
    }
}
