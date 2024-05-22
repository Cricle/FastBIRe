// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Diagnostics.Helpers;
using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    internal abstract class CounterPayload : ICounterPayload
    {
        protected CounterPayload(DateTime timestamp,
            CounterMetadata counterMetadata,
            string displayName,
            string unit,
            double value,
            CounterType counterType,
            float interval,
            int series,
            string valueTags,
            EventType eventType,
            TraceEvent? traceEvent)
        {
            Timestamp = timestamp;
            DisplayName = displayName;
            Unit = unit;
            Value = value;
            CounterType = counterType;
            CounterMetadata = counterMetadata;
            Interval = interval;
            Series = series;
            ValueTags = valueTags;
            EventType = eventType;
            TraceEvent = traceEvent;
        }

        public string DisplayName { get; protected set; }

        public string Unit { get; }

        public double Value { get; }

        public DateTime Timestamp { get; }

        public float Interval { get; }

        public CounterType CounterType { get; }

        public CounterMetadata CounterMetadata { get; }

        public string ValueTags { get; }

        public EventType EventType { get; set; }

        public virtual bool IsMeter => false;

        public int Series { get; }

        public TraceEvent TraceEvent { get; }

        public string? Name => ((IDictionary<string, object>?)((IDictionary<string, object>?)TraceEvent?.PayloadValue(0))?["Payload"])?["Name"]?.ToString();
    }

    internal sealed class EventCounterPayload : CounterPayload
    {
        public EventCounterPayload(DateTime timestamp,
            string providerName,
            string name,
            string displayName,
            string unit,
            double value,
            CounterType counterType,
            float interval,
            int series,
            string valueTags,
            TraceEvent? traceEvent) : base(timestamp, new(providerName, name, null, null, null), displayName, unit, value, counterType, interval, series, valueTags, EventType.Gauge,traceEvent)
        {
        }
    }

    internal abstract class MeterPayload : CounterPayload
    {
        protected MeterPayload(DateTime timestamp,
                    CounterMetadata counterMetadata,
                    string displayName,
                    string unit,
                    double value,
                    CounterType counterType,
                    string valueTags,
                    EventType eventType,
                    TraceEvent traceEvent)
            : base(timestamp, counterMetadata, displayName, unit, value, counterType, 0.0f, 0, valueTags, eventType, traceEvent)
        {
        }

        public override bool IsMeter => true;
    }

    internal sealed class GaugePayload : MeterPayload
    {
        public GaugePayload(CounterMetadata counterMetadata, string displayName, string displayUnits, string valueTags, double value, DateTime timestamp, TraceEvent traceEvent) :
            base(timestamp, counterMetadata, displayName, displayUnits, value, CounterType.Metric, valueTags, EventType.Gauge, traceEvent)
        {
            // In case these properties are not provided, set them to appropriate values.
            string counterName = string.IsNullOrEmpty(displayName) ? counterMetadata.CounterName : displayName;
            DisplayName = !string.IsNullOrEmpty(displayUnits) ? $"{counterName} ({displayUnits})" : counterName;
        }
    }

    internal class UpDownCounterPayload : MeterPayload
    {
        public UpDownCounterPayload(CounterMetadata counterMetadata, string displayName, string displayUnits, string valueTags, double value, DateTime timestamp, TraceEvent traceEvent) :
            base(timestamp, counterMetadata, displayName, displayUnits, value, CounterType.Metric, valueTags, EventType.UpDownCounter, traceEvent)
        {
            // In case these properties are not provided, set them to appropriate values.
            string counterName = string.IsNullOrEmpty(displayName) ? counterMetadata.CounterName : displayName;
            DisplayName = !string.IsNullOrEmpty(displayUnits) ? $"{counterName} ({displayUnits})" : counterName;
        }
    }

    internal sealed class BeginInstrumentReportingPayload : MeterPayload
    {
        public BeginInstrumentReportingPayload(CounterMetadata counterMetadata, DateTime timestamp, TraceEvent traceEvent)
            : base(timestamp, counterMetadata, string.Empty, string.Empty, 0.0, CounterType.Metric, null, EventType.BeginInstrumentReporting,traceEvent)
        {
        }
    }

    internal sealed class CounterEndedPayload : MeterPayload
    {
        public CounterEndedPayload(CounterMetadata counterMetadata, DateTime timestamp, TraceEvent traceEvent)
            : base(timestamp, counterMetadata, string.Empty, string.Empty, 0.0, CounterType.Metric, null, EventType.CounterEnded,traceEvent)
        {
        }
    }

    internal sealed class RatePayload : MeterPayload
    {
        public RatePayload(CounterMetadata counterMetadata, string displayName, string displayUnits, string valueTags, double value, double intervalSecs, DateTime timestamp, TraceEvent traceEvent) :
            base(timestamp, counterMetadata, displayName, displayUnits, value, CounterType.Rate, valueTags, EventType.Rate,traceEvent)
        {
            // In case these properties are not provided, set them to appropriate values.
            string counterName = string.IsNullOrEmpty(displayName) ? counterMetadata.CounterName : displayName;
            string unitsName = string.IsNullOrEmpty(displayUnits) ? "Count" : displayUnits;
            string intervalName = intervalSecs.ToString() + " sec";
            DisplayName = $"{counterName} ({unitsName} / {intervalName})";
        }
    }

    internal record struct Quantile(double Percentage, double Value);

    internal sealed class PercentilePayload : MeterPayload
    {
        public PercentilePayload(CounterMetadata counterMetadata, string displayName, string displayUnits, string valueTags, double value, DateTime timestamp, TraceEvent traceEvent) :
            base(timestamp, counterMetadata, displayName, displayUnits, value, CounterType.Metric, valueTags, EventType.Histogram, traceEvent)
        {
            // In case these properties are not provided, set them to appropriate values.
            string counterName = string.IsNullOrEmpty(displayName) ? counterMetadata.CounterName : displayName;
            DisplayName = !string.IsNullOrEmpty(displayUnits) ? $"{counterName} ({displayUnits})" : counterName;
        }
    }

    // Dotnet-monitor and dotnet-counters previously had incompatible PercentilePayload implementations before being unified -
    // Dotnet-monitor created a single payload that contained all of the quantiles to keep them together, whereas
    // dotnet-counters created a separate payload for each quantile (multiple payloads per TraceEvent).
    // AggregatePercentilePayload allows dotnet-monitor to construct a PercentilePayload for individual quantiles
    // like dotnet-counters, while still keeping the quantiles together as a unit.
    internal sealed class AggregatePercentilePayload : MeterPayload
    {
        public AggregatePercentilePayload(CounterMetadata counterMetadata, string displayName, string displayUnits, string valueTags, IEnumerable<Quantile> quantiles, DateTime timestamp, TraceEvent traceEvent) :
            base(timestamp, counterMetadata, displayName, displayUnits, 0.0, CounterType.Metric, valueTags, EventType.Histogram, traceEvent)
        {
            //string counterName = string.IsNullOrEmpty(displayName) ? name : displayName;
            //DisplayName = !string.IsNullOrEmpty(displayUnits) ? $"{counterName} ({displayUnits})" : counterName;
            Quantiles = quantiles.ToArray();
        }

        public Quantile[] Quantiles { get; }
    }

    internal sealed class ErrorPayload : MeterPayload
    {
        public ErrorPayload(string errorMessage, DateTime timestamp, EventType eventType, TraceEvent traceEvent)
            : base(timestamp, new(), string.Empty, string.Empty, 0.0, CounterType.Metric, null, eventType,traceEvent)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; }
    }

    public enum EventType : int
    {
        Rate,
        Gauge,
        Histogram,
        UpDownCounter,
        BeginInstrumentReporting,
        CounterEnded,
        HistogramLimitError,
        TimeSeriesLimitError,
        ErrorTargetProcess,
        MultipleSessionsNotSupportedError,
        MultipleSessionsConfiguredIncorrectlyError,
        ObservableInstrumentCallbackError
    }

    internal static class CounterPayloadExtensions
    {
        public static string CombineTags(this ICounterPayload counterPayload)
        {
            StringBuilder builder = new();
            builder.Append(counterPayload.CounterMetadata.MeterTags);

            if (!string.IsNullOrEmpty(counterPayload.CounterMetadata.InstrumentTags))
            {
                if (builder.Length > 0)
                {
                    builder.Append(',');
                }
                builder.Append(counterPayload.CounterMetadata.InstrumentTags);
            }

            if (!string.IsNullOrEmpty(counterPayload.ValueTags))
            {
                if (builder.Length > 0)
                {
                    builder.Append(',');
                }
                builder.Append(counterPayload.ValueTags);
            }

            return builder.ToString();
        }
    }

    internal static class EventTypeExtensions
    {
        public static bool IsValuePublishedEvent(this EventType eventType)
        {
            return eventType is EventType.Gauge
                || eventType is EventType.Rate
                || eventType is EventType.Histogram
                || eventType is EventType.UpDownCounter;
        }

        public static bool IsError(this EventType eventType)
        {
            return eventType is EventType.HistogramLimitError
                || eventType is EventType.TimeSeriesLimitError
                || eventType is EventType.ErrorTargetProcess
                || eventType is EventType.MultipleSessionsNotSupportedError
                || eventType is EventType.MultipleSessionsConfiguredIncorrectlyError
                || eventType is EventType.ObservableInstrumentCallbackError;
        }

        public static bool IsNonFatalError(this EventType eventType)
        {
            return IsError(eventType)
                && !IsTracingError(eventType)
                && !IsSessionStartupError(eventType);
        }

        public static bool IsTracingError(this EventType eventType)
        {
            return eventType is EventType.ErrorTargetProcess;
        }

        public static bool IsSessionStartupError(this EventType eventType)
        {
            return eventType is EventType.MultipleSessionsNotSupportedError
                || eventType is EventType.MultipleSessionsConfiguredIncorrectlyError;
        }
    }
    public class CounterMetadata
    {
        public CounterMetadata(string providerName, string counterName, string meterTags, string instrumentTags, string scopeHash)
        {
            ProviderName = providerName;
            CounterName = counterName;
            MeterTags = meterTags;
            InstrumentTags = instrumentTags;
            ScopeHash = scopeHash;
        }

        public CounterMetadata() { }
        public string ProviderName { get; private set; }
        public string CounterName { get; private set; }
        public string MeterTags { get; private set; }
        public string InstrumentTags { get; private set; }
        public string ScopeHash { get; private set; }
    }
}
