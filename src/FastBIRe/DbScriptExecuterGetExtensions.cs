using DatabaseSchemaReader;
using System.Data.Common;

namespace FastBIRe
{
    public static class DbScriptExecuterGetExtensions
    {
        public static DatabaseReader CreateReader(this IDbScriptExecuter dbScriptExecuter)
        {
            return new DatabaseReader(dbScriptExecuter.Connection) { Owner = dbScriptExecuter.Connection.Database };
        }
        public static DefaultScriptExecuter CreateDefaultExecuter(this DbConnection connection)
        {
            return new DefaultScriptExecuter(connection);
        }
        public static DatabaseReader CreateReader(this DbConnection connection)
        {
            return new DatabaseReader(connection) { Owner = connection.Database };
        }
    }
}
