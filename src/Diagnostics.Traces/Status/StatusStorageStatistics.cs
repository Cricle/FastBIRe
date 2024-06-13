namespace Diagnostics.Traces.Status
{
    public class StatusStorageStatistics:IStatusStorageStatistics
    {
        private long scopeCount;
        private long notFailCount;
        private long failCount;
        private long totalCount;

        public long ScopeCount =>Interlocked.Read(ref scopeCount);

        public long NotFailCount => Interlocked.Read(ref notFailCount);

        public long FailCount => Interlocked.Read(ref failCount);

        public long TotalCount => Interlocked.Read(ref totalCount);

        public virtual bool AddScope(IStatusScope scope)
        {
            Interlocked.Increment(ref scopeCount);
            return true;
        }

        public virtual bool ComplatedScope(IStatusScope scope, StatusTypes types)
        {
            Interlocked.Decrement(ref scopeCount);
            Interlocked.Increment(ref totalCount);
            switch (types)
            {
                case StatusTypes.Unset:
                case StatusTypes.Succeed:
                    Interlocked.Increment(ref notFailCount);
                    break;
                case StatusTypes.Fail:
                    Interlocked.Increment(ref failCount);
                    break;
                default:
                    break;
            }
            return true;
        }
    }
}
