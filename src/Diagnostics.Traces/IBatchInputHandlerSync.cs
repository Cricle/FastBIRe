using OpenTelemetry;

namespace Diagnostics.Traces
{
    public interface IBatchInputHandlerSync<T>
        where T : class
    {
        void Handle(in Batch<T> inputs);
    }
}
