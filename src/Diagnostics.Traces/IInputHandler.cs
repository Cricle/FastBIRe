using OpenTelemetry;

namespace Diagnostics.Traces
{
    public interface IInputHandler<T>
    {
        Task HandleAsync(T input, CancellationToken token);
    }
}
