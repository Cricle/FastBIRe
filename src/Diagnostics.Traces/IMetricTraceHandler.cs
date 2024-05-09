using OpenTelemetry.Metrics;

namespace Diagnostics.Traces
{
    public interface IMetricTraceHandler : IInputHandler<Metric>
    {
    }
}
