using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.Mssql
{
    public class MssqlGetCdcListenerOptions : GetCdcListenerOptions
    {
        public MssqlGetCdcListenerOptions(TimeSpan delayScan, IDbScriptExecuter scriptExecuter, IReadOnlyList<string>? tableNames,ICheckpoint? checkpoint)
            :base(tableNames,checkpoint)
        {
            DelayScan = delayScan;
            ScriptExecuter = scriptExecuter;
        }
        public IDbScriptExecuter ScriptExecuter { get; }

        public TimeSpan DelayScan { get; }
    }
}
