namespace FastBIRe.Cdc.Mssql
{
    public class MssqlGetCdcListenerOptions : IGetCdcListenerOptions
    {
        public MssqlGetCdcListenerOptions(IReadOnlyList<string>? tableNames, TimeSpan delayScan, IDbScriptExecuter scriptExecuter)
        {
            TableNames = tableNames;
            DelayScan = delayScan;
            ScriptExecuter = scriptExecuter;
        }
        public IDbScriptExecuter ScriptExecuter { get; }

        public IReadOnlyList<string>? TableNames { get; }

        public TimeSpan DelayScan { get; }
    }
}
