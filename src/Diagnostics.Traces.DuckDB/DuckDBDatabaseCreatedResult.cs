using Diagnostics.Traces.Stores;
using DuckDB.NET.Data;
using System.Data.Common;

namespace Diagnostics.Traces.DuckDB
{
    internal static class ConnectionExecuteExtensions
    {
        public static int ExecuteNoQuery(this DbConnection connection,string sql)
        {
            using (var comm = connection.CreateCommand())
            {
                comm.CommandText = sql;
                return comm.ExecuteNonQuery();
            }
        }
    }

    public class DuckDBDatabaseCreatedResult : IDatabaseCreatedResult,IDisposable
    {
        public DuckDBDatabaseCreatedResult(DuckDBConnection connection, string? filePath)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            FilePath = filePath;
            Root = new object();
        }

        public object Root { get; }

        public DuckDBConnection Connection { get; }

        public string? FilePath { get; }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}
