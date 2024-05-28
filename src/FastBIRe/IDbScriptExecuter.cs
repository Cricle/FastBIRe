using DatabaseSchemaReader.DataSchema;
using System.Data.Common;

namespace FastBIRe
{
    public interface IDbScriptExecuter : IDbScriptTransaction, IScriptExecuter
    {
        SqlType SqlType { get; }

        DbConnection Connection { get; }
    }
}
