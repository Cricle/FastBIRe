using Diagnostics.Traces.Models;

namespace Diagnostics.Traces
{
    public interface IDuckDBTraceReader
    {
        IEnumerable<AcvtityEntity> ReadActivities();
        IEnumerable<ExceptionEntity> ReadExceptions();
        IEnumerable<LogEntity> ReadLogs();
        IEnumerable<MetricEntity> ReadMetrics();
    }
}
