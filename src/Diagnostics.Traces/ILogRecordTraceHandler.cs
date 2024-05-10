using OpenTelemetry.Logs;

namespace Diagnostics.Traces
{
    public interface ILogRecordTraceHandler : IInputHandler<LogRecord>,IInputHandlerSync<LogRecord>
    {
    }
    public interface IBatchLogRecordTraceHandler : IBatchInputHandler<LogRecord>, IBatchInputHandlerSync<LogRecord>
    {
    }
}
