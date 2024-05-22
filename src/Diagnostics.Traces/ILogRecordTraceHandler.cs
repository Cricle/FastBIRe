using Diagnostics.Generator.Core;
using OpenTelemetry.Logs;

namespace Diagnostics.Traces
{
    public interface ILogRecordTraceHandler : IOpetatorHandler<LogRecord>,IInputHandlerSync<LogRecord>
    {
    }
    public interface IBatchLogRecordTraceHandler : IBatchInputHandler<LogRecord>, IBatchInputHandlerSync<LogRecord>
    {
    }
}
