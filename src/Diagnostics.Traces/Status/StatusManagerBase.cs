using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.Status
{
    public abstract class StatusManagerBase : IStatusManager
    {
        public abstract Task<long> CleanAsync(string name, CancellationToken token = default);
        public abstract Task<long> CleanBeforeAsync(string name, DateTime time, CancellationToken token = default);

        public abstract long? Count(string name);
        public virtual Task<long?> CountAsync(string name, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(Count(name));
        }
        public abstract IStatusScope CreateScope(string name);
        public virtual Task<IStatusScope> CreateScopeAsync(string name, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(CreateScope(name));
        }
        public abstract void Dispose();

        public abstract IEnumerable<StatusInfo> Find(string name, DateTime? leftTime = null, DateTime? rightTime = null);
        public virtual async IAsyncEnumerable<StatusInfo> FindAsync(string name, DateTime? leftTime = null, DateTime? rightTime = null, [EnumeratorCancellation]CancellationToken token = default)
        {
            await Task.Yield();
            token.ThrowIfCancellationRequested();
            foreach (var item in Find(name,leftTime,rightTime))
            {
                yield return item;
            }
        }
        
        public abstract StatusInfo? Find(string name, string key);
        public virtual Task<StatusInfo?> FindAsync(string name, string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(Find(name,key));
        }
        public abstract IReadOnlyList<string> GetNames();
        public virtual Task<IReadOnlyList<string>> GetNamesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(GetNames());
        }
        public abstract bool Initialize(string name);
        public virtual Task<bool> InitializeAsync(string name, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(Initialize(name));
        }
    }
}