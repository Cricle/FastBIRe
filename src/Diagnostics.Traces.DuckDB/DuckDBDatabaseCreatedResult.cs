using Diagnostics.Traces.Stores;
using DuckDB.NET.Native;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBDatabaseCreatedResult : IDatabaseCreatedResult,IDisposable
    {
        public DuckDBDatabaseCreatedResult(DuckDBDatabase database, string? filePath)
        {
            var status = NativeMethods.Startup.DuckDBConnect(database, out var conn);
            if (status!= DuckDBState.Success)
            {
                throw new TraceDuckDbException($"Fail to open duckdb {filePath}");
            }

            Connection = conn;

            Database = database;
            FilePath = filePath;
            Root = new object();
        }

        public object Root { get; }

        public DuckDBNativeConnection Connection { get; }

        public DuckDBDatabase Database { get; }

        public string? FilePath { get; }

        public void Dispose()
        {
            Connection?.Dispose();
            Database?.Dispose();
        }
    }
}
