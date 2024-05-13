using System.Data.Common;

namespace Diagnostics.Traces.DuckDB
{
    internal static class DbExtensions
    {
        public static DbDataReader Query(this DbConnection connection, string sql, int? timeout = null, DbTransaction? transaction = null)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }
                if (timeout != null)
                {
                    command.CommandTimeout = timeout.Value;
                }
                return command.ExecuteReader();
            }
        }
        public static int Execute(this DbConnection connection, string sql, int? timeout = null, DbTransaction? transaction = null, bool noAsync = false)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }
                if (timeout != null)
                {
                    command.CommandTimeout = timeout.Value;
                }
                return command.ExecuteNonQuery();
            }
        }
    }
}
