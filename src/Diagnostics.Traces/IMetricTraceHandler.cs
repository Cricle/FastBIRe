using Diagnostics.Generator.Core;
using OpenTelemetry.Metrics;

namespace Diagnostics.Traces
{
    public interface IMetricTraceHandler : IOpetatorHandler<Metric>,IInputHandlerSync<Metric>
    {
    }
    public interface IBatchMetricTraceHandler : IBatchInputHandler<Metric>, IBatchInputHandlerSync<Metric>
    {
    }
}
