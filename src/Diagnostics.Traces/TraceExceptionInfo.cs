using System.Diagnostics;

namespace Diagnostics.Traces
{
    public readonly record struct TraceExceptionInfo
    {
        public TraceExceptionInfo(Exception exception, ActivityTraceId? traceId, ActivitySpanId? spanId)
        {
            Exception = exception;
            TraceId = traceId;
            SpanId = spanId;
            CreateTime = DateTime.Now;
        }

        public Exception Exception { get; }

        public ActivityTraceId? TraceId { get; }

        public ActivitySpanId? SpanId { get; }

        public DateTime CreateTime { get; }
    }
}
