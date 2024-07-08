using Diagnostics.Traces.Stores;
using DuckDB.NET.Data;
using DuckDB.NET.Native;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBDatabaseCreatedResult : IDatabaseCreatedResult, IDisposable
    {
        public DuckDBDatabaseCreatedResult(DuckDBConnection connection, string? filePath, string key)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            FilePath = filePath;
            Root = new object();
            Key = key;
        }

        private int disposedCount;

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

        public SaveLogModes SaveLogModes { get; set; } = SaveLogModes.All;

        public SaveExceptionModes SaveExceptionModes { get; set; } = SaveExceptionModes.All;

        public string? FilePath { get; }

        public string Key { get; }

        public void Dispose()
        {
            if (Interlocked.Increment(ref disposedCount) == 1)
            {
                Connection.Dispose();
            }
        }
    }
}
