namespace Diagnostics.Traces.Status
{
    public abstract class StatusScopeBase : IStatusScope
    {
        private long isComplated;

        public abstract string Name { get; }

        public bool IsComplated => Interlocked.Read(ref isComplated)!=0;

        public bool Complate(StatuTypes types = StatuTypes.Unset)
        {
            if (Interlocked.CompareExchange(ref isComplated,1,0)==0)
            {
                OnComplate(types);
                return true;
            }
            return false;
        }
        protected void ThrowIfComplated()
        {
            if (IsComplated)
            {
                throw new InvalidOperationException($"The status scope is comaplted, can't operator");
            }
        }
        protected abstract void OnComplate(StatuTypes types = StatuTypes.Unset);

        public virtual Task<bool> ComplateAsync(StatuTypes types = StatuTypes.Unset, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(Complate(types));
        }

        public abstract void Dispose();

        public virtual ValueTask DisposeAsync()
        {
            Dispose();
            return new ValueTask();
        }

        public abstract bool Log(string message);

        public Task<bool> LogAsync(string message, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(Log(message));
        }

        public abstract bool Set(string status);

        public Task<bool> SetAsync(string status, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(Set(status));
        }
    }
}