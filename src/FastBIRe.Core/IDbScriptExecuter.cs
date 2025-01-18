
using System.Data.Common;

namespace FastBIRe
{
    public interface IDbScriptExecuter : IDbScriptTransaction, IScriptExecuter
    {
        FSqlType SqlType { get; }

        DbConnection Connection { get; }
    }
}
