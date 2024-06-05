using Diagnostics.Traces.DuckDB.Status;
using Diagnostics.Traces.Stores;
using DuckDB.NET.Data;
using DuckDB.NET.Native;
using System.Data.Common;

namespace Diagnostics.Traces.DuckDB
{
    internal static class ConnectionExecuteExtensions
    {
        public static int ExecuteNoQuery(this DbConnection connection, string sql)
        {
            using (var comm = connection.CreateCommand())
            {
                comm.CommandText = sql;
                return comm.ExecuteNonQuery();
            }
        }
    }

    public class DuckDBDatabaseCreatedResult : IDatabaseCreatedResult, IDisposable
    {
        public DuckDBDatabaseCreatedResult(DuckDBConnection connection, string? filePath, string key)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            FilePath = filePath;
            Root = new object();
            Key = key;
        }

        private DuckDBNativeConnection? nativeConnection;

        public object Root { get; }

        public DuckDBConnection Connection { get; }

        public DuckDBNativeConnection NativeConnection
        {
            get
            {
                if (nativeConnection == null)
                {
                    nativeConnection = DuckDBNativeHelper.GetNativeConnection(Connection);
                }
                return nativeConnection;
            }
        }

        public string? FilePath { get; }

        public string Key { get; }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}
