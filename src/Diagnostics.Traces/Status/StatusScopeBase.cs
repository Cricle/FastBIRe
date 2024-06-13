namespace Diagnostics.Traces.Status
{
    public abstract class StatusScopeBase : IStatusScope
    {
        private long isComplated;

        protected readonly IList<TimePairValue> logs;
        protected readonly IList<TimePairValue> status;

        protected StatusScopeBase(DateTime time)
        {
            logs = new List<TimePairValue>();
            status = new List<TimePairValue>();
            StartTime = time;
        }

        public abstract string Key { get; }

        public bool IsComplated => Interlocked.Read(ref isComplated) != 0;

        public abstract string Name { get; }

        public virtual DateTime StartTime { get; }

        public bool Complate(StatusTypes types = StatusTypes.Unset)
        {
            if (Interlocked.CompareExchange(ref isComplated, 1, 0) == 0)
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
        protected abstract void OnComplate(StatusTypes types = StatusTypes.Unset);

        public virtual Task<bool> ComplateAsync(StatusTypes types = StatusTypes.Unset, CancellationToken token = default)
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

        public virtual bool Log(string message)
        {
            if (Interlocked.Read(ref isComplated) != 0)
            {
                return false;
            }
            logs.Add(new TimePairValue(message));
            return true;
        }

        public virtual bool Set(string status)
        {
            if (Interlocked.Read(ref isComplated) != 0)
            {
                return false;
            }
            this.status.Add(new TimePairValue(status));
            return true;
        }
    }
}