using System.Data.Common;

namespace FastBIRe.Project
{
    public class DbConnectionBox : IDisposable
    {
        public DbConnectionBox(DbConnectionPool pool)
        {
            Pool = pool;
            Connection = pool.Get();
        }

        public DbConnectionPool Pool { get; }

        public DbConnection Connection { get; }

        public void Dispose()
        {
            Pool.Return(Connection);
        }
    }
}
