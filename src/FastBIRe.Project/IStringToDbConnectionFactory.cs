using DatabaseSchemaReader.DataSchema;
using System.Data.Common;

namespace FastBIRe.Project
{
    public interface IStringToDbConnectionFactory
    {
        SqlType SqlType { get; }

        DbConnection CreateDbConnection(string connectionString, string database);

        DbConnection CreateDbConnection(string connectionString);
    }
    public class DelegateStringToDbConnectionFactory : IStringToDbConnectionFactory
    {
        public DelegateStringToDbConnectionFactory(SqlType sqlType, Func<string, DbConnection> dbConnection, Func<string, string, DbConnection> dbConnectionWithDatabase)
        {
            SqlType = sqlType;
            DbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            DbConnectionWithDatabase = dbConnectionWithDatabase ?? throw new ArgumentNullException(nameof(dbConnectionWithDatabase));
        }

        public SqlType SqlType { get; }

        public Func<string, DbConnection> DbConnection { get; }

        public Func<string, string, DbConnection> DbConnectionWithDatabase { get; }

        public DbConnection CreateDbConnection(string connectionString, string database)
        {
            return DbConnectionWithDatabase(connectionString, database);
        }

        public DbConnection CreateDbConnection(string connectionString)
        {
            return DbConnection(connectionString);
        }
    }
}
