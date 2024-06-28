namespace Diagnostics.Traces
{
    public enum SaveLogModes
    {
        Timestamp = 1,
        LogLevel = Timestamp << 1,
        CategoryName = Timestamp << 2,
        TraceId = Timestamp << 3,
        SpanId = Timestamp << 4,
        Attributes = Timestamp << 5,
        FormattedMessage = Timestamp << 6,
        Body = Timestamp << 7,
        Mini = Timestamp | TraceId | SpanId | FormattedMessage,
        All = Timestamp | LogLevel | CategoryName | TraceId | SpanId | Attributes | FormattedMessage | Body
    }
}
