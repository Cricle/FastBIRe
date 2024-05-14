; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DG0001 | Diagnostics.Generator | Error | Unsupport event source type, [Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource.writeeventcore?view=net-8.0)
DG0002 | Diagnostics.Generator | Error | Related activity id only one or zero
DG0003 | Diagnostics.Generator | Error | Related activity id must guid
DG0004 | Diagnostics.Generator | Error | Field type must int, long, double, float,Enum, IncrementingEventCounter, EventCounter
DG0005 | Diagnostics.Generator | Error | Counter type is IncrementingEventCounter, EventCounter, the field type must be that
DG0006 | Diagnostics.Generator | Error | IncrementingPollingCounter, IncrementingEventCounter must input DisplayRateTimeScaleMs and more than zero
DG0007 | Diagnostics.Generator | Warning | PollingCounter or IncrementingPollingCounter field type must long, double, float
DG0008 | Diagnostics.Generator | Info | The event source name recommendation end with EventSource, such as ProcessEventSource
DG0009 | Diagnostics.Generator | Info | Auto write event body
DG0010 | Diagnostics.Generator | Error | When ForAnysProviders=true, must input ForProviders
DG0011 | Diagnostics.Generator | Warning | The id \"{0}\" has same
DG0012 | Diagnostics.Generator | Warning | The WithCreator is true, but found nothing providers
DG0013 | Diagnostics.Generator | Error | The EventLevel \"{0}\" is not support
DG0014 | Diagnostics.Generator | Error | Fail to parse arguments \"{0}\", the Arguments must like interval=123,a1=2 or \"interval\"=\"123\",\"a1\"=\"2\"
DG0015 | Diagnostics.Generator | Error | The EventSourceAccesstorInstanceAttribute only can be zero or one
DG0016 | Diagnostics.Generator | Error | The AccesstorInstance must static interval/public
DG0017 | Diagnostics.Generator | Error | The event source \"{0}\" has no GenerateSingleton or tag EventSourceAccesstorInstanceAttribute field or property, can't set WithCalledTogetherExtensions = ture
DG0018 | Diagnostics.Generator | Error | The meter member \"{0}\" was not found, please check the type \"{1}\" has any this member
DG0019 | Diagnostics.Generator | Error | The meter member type \"{0}\" was unknow, now avaliable types is Counter, Histogram and UpDownCounter
DG0020 | Diagnostics.Generator | Error | The meter method first paramter must same as counter generic type \"{0}\"
DG0021 | Diagnostics.Generator | Error | The meter method must return void and not generic method
DG0022 | Diagnostics.Generator | Error | The \"{0}\" is not class or struct
DG0023 | Diagnostics.Generator | Error | The \"{0}\" is loop reference, path is \"{1}\", the generator can't parse loop reference
DG0024 | Diagnostics.Generator | Error | Tag as self must not static