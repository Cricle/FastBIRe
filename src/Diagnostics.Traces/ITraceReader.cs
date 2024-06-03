using Diagnostics.Traces.Models;

namespace Diagnostics.Traces
{
    public interface ITraceReader
    {
        IEnumerable<AcvtityEntity> ReadActivities(IEnumerable<string>? traceIds = null);

        IEnumerable<ExceptionEntity> ReadExceptions(IEnumerable<string>? traceIds = null);

        IEnumerable<LogEntity> ReadLogs(IEnumerable<string>? traceIds = null);

        IEnumerable<MetricEntity> ReadMetrics();
    }
}
