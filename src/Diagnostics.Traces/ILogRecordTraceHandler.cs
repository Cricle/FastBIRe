using OpenTelemetry.Logs;

namespace Diagnostics.Traces
{
    public interface ILogRecordTraceHandler : IInputHandler<LogRecord>,IInputHandlerSync<LogRecord>
    {
    }
}
