using Diagnostics.Traces.Models;

namespace Diagnostics.Traces
{
    public static class TraceReaderTreeExtensions
    {
        public static TraceData CreateTraceData(this ITraceReader reader)
        {
            var activities = reader.ReadActivities();
            var logs = reader.ReadLogs();
            var exceptions = reader.ReadExceptions();
            return TraceData.Create(logs, activities, exceptions);
        }
    }
}
