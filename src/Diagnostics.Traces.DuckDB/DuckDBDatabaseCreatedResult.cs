using Diagnostics.Traces.Stores;
using DuckDB.NET.Data;
using DuckDB.NET.Native;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBDatabaseCreatedResult : DatabaseCreatedResultBase
    {
        public DuckDBDatabaseCreatedResult(DuckDBConnection connection, string? filePath, string key)
            :base(filePath,key)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        private DuckDBNativeConnection? nativeConnection;

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

        protected override void OnDisposed()
        {
            Connection.Dispose();
        }
    }
}
