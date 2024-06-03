namespace Diagnostics.Traces.Models
{
    public class ExceptionEntity:ITraceKeyProvider
    {
        public string? TraceId { get; set; }

        public string? SpanId { get; set; }

        public DateTime CreateTime { get; set; }

        public string? TypeName { get; set; }

        public string? Message { get; set; }

        public string? HelpLink { get; set; }

        public int HResult { get; set; }

        public Dictionary<string, string>? Data { get; set; }

        public string? StackTrace { get; set; }

        public string? InnerException { get; set; }


        public TraceKey GetTraceKey()
        {
            return new TraceKey(TraceId, SpanId);
        }
    }
}
