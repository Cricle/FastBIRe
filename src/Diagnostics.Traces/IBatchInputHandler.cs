using OpenTelemetry;

namespace Diagnostics.Traces
{
    public interface IBatchInputHandler<T>
        where T: class
    {
        Task HandleAsync(Batch<T> inputs, CancellationToken token);
    }
}
