using System.Data.Common;

namespace FastBIRe
{
    public interface IDbScriptExecuter : IDbScriptTransaction, IScriptExecuter
    {
        DbConnection Connection { get; }
    }
}
