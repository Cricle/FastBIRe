using Microsoft.Diagnostics.Monitoring.EventPipe;
using Microsoft.Diagnostics.Tracing;
using System;

namespace Diagnostics.Helpers
{
    public interface ICounterPayload
    {
        double Value { get; }

        CounterType CounterType { get; }

        CounterMetadata CounterMetadata { get; }

        string DisplayName { get; }

        string Unit { get; }

        DateTime Timestamp { get; }

        /// <summary>
        /// The interval between counters. Note this is the actual measure of time elapsed, not the requested interval.
        /// </summary>
        float Interval { get; }

        /// <summary>
        /// Optional tags for counters. Note that normal counters use ':' as a separator character, while System.Diagnostics.Metrics use ';'.
        /// We do not immediately convert string to Dictionary, since dotnet-counters does not need this conversion.
        /// </summary>
        string ValueTags { get; }

        EventType EventType { get; }

        bool IsMeter { get; }

        int Series { get; }

        TraceEvent TraceEvent { get; }
    }
}
