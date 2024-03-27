using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.Mssql
{
    public class MssqlGetCdcListenerOptions : GetCdcListenerOptions
    {
        public MssqlGetCdcListenerOptions(TimeSpan delayScan, IDbScriptExecuter scriptExecuter, ICheckpoint? checkpoint)
            : base(checkpoint)
        {
            DelayScan = delayScan;
            ScriptExecuter = scriptExecuter;
        }
        public IDbScriptExecuter ScriptExecuter { get; }

        public TimeSpan DelayScan { get; }
    }
}
