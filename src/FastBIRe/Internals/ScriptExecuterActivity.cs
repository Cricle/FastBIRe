using System.Diagnostics;

namespace FastBIRe.Internals
{
    internal static partial class ScriptExecuterActivity
    {
        public static readonly ActivitySource ScriptExecuterActivitySource = new ActivitySource(ScriptExecuterEventSource.EventName, typeof(ScriptExecuterActivity).Assembly.GetName().Version.ToString());
    }
}
