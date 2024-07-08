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
    public enum SaveExceptionModes
    {
        TraceId = 1,
        SpanId = TraceId << 1,
        CreateTime = TraceId << 2,
        TypeName = TraceId << 3,
        Message = TraceId << 4,
        HelpLink = TraceId << 5,
        HResult = TraceId << 6,
        Data = TraceId << 7,
        StackTrace = TraceId << 8,
        InnerException = TraceId << 9,
        Mini = TraceId | SpanId | Message | StackTrace,
        All = TraceId | SpanId | CreateTime | TypeName | Message | HelpLink | HResult | Data | StackTrace | InnerException
    }
}
