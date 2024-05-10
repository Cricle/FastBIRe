using OpenTelemetry.Metrics;

namespace Diagnostics.Traces
{
    public interface IMetricTraceHandler : IInputHandler<Metric>,IInputHandlerSync<Metric>
    {
    }
    public interface IBatchMetricTraceHandler : IBatchInputHandler<Metric>, IBatchInputHandlerSync<Metric>
    {
    }
}
