namespace Diagnostics.Traces.Status
{
    public interface IStatusStorageStatistics
    {
        long ScopeCount { get; }

        long NotFailCount { get; }

        long FailCount { get; }

        long TotalCount { get; }
    }
}
