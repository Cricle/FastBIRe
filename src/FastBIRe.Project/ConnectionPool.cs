using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;
using System.Data.Common;

namespace FastBIRe.Project
{
    public class DbConnectionPool : ObjectPool<DbConnection>,IDisposable
    {
        public DbConnectionPool(IPooledObjectPolicy<DbConnection> objectPolicy)
        {
            Inner = new DefaultObjectPoolProvider().Create(objectPolicy);
        }
        public DbConnectionPool(ObjectPool<DbConnection> inner)
        {
            Inner = inner;
        }

        public ObjectPool<DbConnection> Inner { get; }

        public void Dispose()
        {
            if (Inner is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public override DbConnection Get()
        {
            return Inner.Get();
        }

        public override void Return(DbConnection obj)
        {
            Inner.Return(obj);
        }
    }
}
