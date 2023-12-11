using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using System.Data.Common;

namespace FastBIRe
{
    public static class DbScriptExecuterGetExtensions
    {
        public static SqlType GetRequiredSqlType(this DbConnection connection)
        {
            var sqlType = GetSqlType(connection) ?? throw new NotSupportedException(connection.GetType().FullName);
            return sqlType;
        }
        public static SqlType? GetSqlType(this DbConnection connection)
        {
            return ProviderToSqlType.Convert(connection.GetType().FullName);
        }
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
