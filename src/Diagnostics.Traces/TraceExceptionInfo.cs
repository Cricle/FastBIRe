using System.Diagnostics;

namespace Diagnostics.Traces
{
    public record struct TraceExceptionInfo
    {
        public TraceExceptionInfo(Exception exception, string? traceId, string? spanId)
        {
            Exception = exception;
            TraceId = traceId;
            SpanId = spanId;
            CreateTime = DateTime.Now;
        }

        public Exception Exception { get; set; }

        public string? TraceId { get; set; }

        public string? SpanId { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
