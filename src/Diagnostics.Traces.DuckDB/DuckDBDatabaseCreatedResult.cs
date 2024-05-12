using Diagnostics.Traces.Stores;
using DuckDB.NET.Data;

namespace Diagnostics.Traces.DuckDB
{
    public readonly struct DuckDBDatabaseCreatedResult : IDatabaseCreatedResult,IDisposable
    {
        public DuckDBDatabaseCreatedResult(DuckDBConnection database, string? filePath)
        {
            Database = database;
            FilePath = filePath;
            Root = new object();
        }

        public object Root { get; }

        public DuckDBConnection Database { get; }

        public string? FilePath { get; }

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
