namespace FastBIRe.Cdc.Mssql
{
    public class TriggerGetCdcListenerOptions : IGetCdcListenerOptions
    {
        public TriggerGetCdcListenerOptions(IDbScriptExecuter scriptExecuter, IReadOnlyList<string> tableNames, TimeSpan delayScan, uint readBatch)
        {
            TableNames = tableNames;
            DelayScan = delayScan;
            ScriptExecuter = scriptExecuter;
            ReadBatch = readBatch;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public IReadOnlyList<string> TableNames { get; }

        public TimeSpan DelayScan { get; }

        public uint ReadBatch { get; }
    }
}
