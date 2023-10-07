using System.Data.Common;

namespace FastBIRe.Data
{
    public readonly record struct SQLMirrorTarget
    {
        public SQLMirrorTarget(IScriptExecuter scriptExecuter, string named)
        {
            ScriptExecuter = scriptExecuter ?? throw new ArgumentNullException(nameof(scriptExecuter));
            Named = named ?? throw new ArgumentNullException(nameof(named));
        }

        public IScriptExecuter ScriptExecuter { get; }

        public string Named { get; }
    }
}
