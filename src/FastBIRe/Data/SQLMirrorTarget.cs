using System.Data.Common;

namespace FastBIRe.Data
{
    public readonly record struct SQLMirrorTarget : IDisposable
    {
        public SQLMirrorTarget(DbConnection connection, string named)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Named = named ?? throw new ArgumentNullException(nameof(named));
        }

        public DbConnection Connection { get; }

        public string Named { get; }

        public void Dispose()
        {
            Connection?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
