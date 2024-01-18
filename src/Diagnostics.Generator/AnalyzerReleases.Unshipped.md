; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DG0001 | Diagnostics.Generator | Error | Unsupport event source type, [Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource.writeeventcore?view=net-8.0)
DG0002 | Diagnostics.Generator | Error | Related activity id only one or zero
DG0003 | Diagnostics.Generator | Error | Related activity id must guid
DG0004 | Diagnostics.Generator | Error | Field type must int, long, double, float, IncrementingEventCounter, EventCounter
DG0005 | Diagnostics.Generator | Error | Counter type is IncrementingEventCounter, EventCounter, the field type must be that
DG0006 | Diagnostics.Generator | Error | IncrementingPollingCounter, IncrementingEventCounter must input DisplayRateTimeScaleMs and more than zero
DG0007 | Diagnostics.Generator | Error | PollingCounter or IncrementingPollingCounter field type must long, double, float
DG0008 | Diagnostics.Generator | Info | The event source name recommendation end with EventSource, such as ProcessEventSource