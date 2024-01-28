using Microsoft.CodeAnalysis;

namespace Diagnostics.Generator
{
    internal static class Messages
    {
        public const string Category = "Diagnostics.Generator";

        public static readonly DiagnosticDescriptor NotSupportType = new DiagnosticDescriptor("DG0001",
            "EventSourceGenerator",
            "Unsupport event source type", Category,
             DiagnosticSeverity.Error, true, helpLinkUri: "https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource.writeeventcore?view=net-8.0");

        public static readonly DiagnosticDescriptor RelatedActivityIdOnlyOne = new DiagnosticDescriptor("DG0002",
            "EventSourceGenerator",
            "Related activity id only one or zero", Category,
             DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor RelatedActivityIdMustGuid = new DiagnosticDescriptor("DG0003",
            "EventSourceGenerator",
            "Related activity id must guid", Category,
             DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor FieldMustReturnNumber = new DiagnosticDescriptor("DG0004",
            "EventSourceGenerator",
            "Field type must int, long, double, float, Enum, IncrementingEventCounter, EventCounter", Category,
             DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor EventCounterMustType = new DiagnosticDescriptor("DG0005",
            "EventSourceGenerator",
            "Counter type is IncrementingEventCounter, EventCounter, the field type must be that", Category,
             DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor PollingCounterMustInputRate = new DiagnosticDescriptor("DG0006",
            "EventSourceGenerator",
            "IncrementingPollingCounter, IncrementingEventCounter must input DisplayRateTimeScaleMs and more than zero", Category,
             DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor PollingCounterMustSimpleType = new DiagnosticDescriptor("DG0007",
            "EventSourceGenerator",
            "PollingCounter or IncrementingPollingCounter field type must int, long, double, float", Category,
             DiagnosticSeverity.Warning, true);
        public static readonly DiagnosticDescriptor NameNeedEndWithEventSource = new DiagnosticDescriptor("DG0008",
            "The event source name recommendation end with EventSource, such as ProcessEventSource",
            "The event source name recommendation end with EventSource, such as ProcessEventSource", Category,
             DiagnosticSeverity.Info, true);
        public static readonly DiagnosticDescriptor WriteEventBody = new DiagnosticDescriptor("DG0009",
            "Auto write event body",
            "Auto write event body", Category,
             DiagnosticSeverity.Info, true);
    }
}
