using System.Data.Common;

namespace FastBIRe.Project
{
    public class DelegateDbConnectionFactory : IDbConnectionFactory
    {
        public DelegateDbConnectionFactory(Func<DbConnection> func)
        {
            Func = func;
        }

        public Func<DbConnection> Func { get; }

        public DbConnection CreateConnection()
        {
            return Func();
        }
    }
}
