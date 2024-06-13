using System.Collections;
using System.Collections.Concurrent;

namespace Diagnostics.Traces.Status
{
    public class InMemoryStatusStorage : StatusStorageStatistics,IStatusStorage
    {
        private readonly ConcurrentDictionary<string, IStatusScope> statusScopes = new ConcurrentDictionary<string, IStatusScope>();

        public InMemoryStatusStorage(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override bool AddScope(IStatusScope scope)
        {
            if (scope.Name == Name && statusScopes.TryAdd(scope.Key, scope))
            {
                base.AddScope(scope);
                return true;
            }
            return false;
        }
        public override bool ComplatedScope(IStatusScope scope, StatusTypes types)
        {
            if (scope.Name == Name && statusScopes.TryRemove(scope.Key, out _))
            {
                base.ComplatedScope(scope, types);
                return true;
            }
            return false;
        }

        public IEnumerator<IStatusScope> GetEnumerator()
        {
            return statusScopes.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
