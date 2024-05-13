using Diagnostics.Traces.Stores;
using LiteDB;

namespace Diagnostics.Traces.LiteDb
{
    public class LiteDatabaseCreatedResult : IDatabaseCreatedResult,IDisposable
    {
        public LiteDatabaseCreatedResult(ILiteDatabase database, string? filePath)
        {
            Database = database;
            FilePath = filePath;
            Root = new object();
        }

        public object Root { get; }

        public ILiteDatabase Database { get; }

        public string? FilePath { get; }

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
