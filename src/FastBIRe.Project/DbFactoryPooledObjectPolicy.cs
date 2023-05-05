using Microsoft.Extensions.ObjectPool;
using System.Data.Common;

namespace FastBIRe.Project
{
    public class DbFactoryPooledObjectPolicy : IPooledObjectPolicy<DbConnection>
    {
        public DbFactoryPooledObjectPolicy(IDbConnectionFactory factory)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public IDbConnectionFactory Factory { get; }

        public virtual DbConnection Create()
        {
            return Factory.CreateConnection();
        }

        public virtual bool Return(DbConnection obj)
        {
            return true;
        }
    }
}
