// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Text;

namespace Diagnostics.Helpers
{
    public class CounterConfiguration
    {
        public CounterConfiguration(CounterFilter filter)
        {
            CounterFilter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public CounterFilter CounterFilter { get; }

        public string SessionId { get; set; }

        public string ClientId { get; set; }

        public int MaxHistograms { get; set; }

        public int MaxTimeseries { get; set; }
    }

    internal record struct ProviderAndCounter(string ProviderName, string CounterName);
    public static class InnerEventExtensions
    {
        public static bool TryGetCounterPayload<T>(this Instrument instrument, T measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, out ICounterPayload? payload)
        {
            double value;
            if (measurement is double d)
            {
                value = d;
            }
            else
            {
                value = (double)Convert.ChangeType(measurement, typeof(double))!;
            }

            payload = new EventCounterPayload(
                    DateTime.Now,
                    instrument.Name,
                    instrument.Name,
                    instrument.Description,
                    instrument.Unit,
                    value,
                    CounterType.Metric,
                    -1,
                    -1,
                    MakeTagString(instrument.Tags, tags),
                    null,
                    null);

            return true;
        }
        private static void AppendItem(StringBuilder s, KeyValuePair<string, object?> item)
        {
            s.Append('\"');
            s.Append(item.Key);
            s.Append("\",");
            if (item.Value == null)
            {
                s.Append("NULL");
            }
            else
            {
                s.Append('\"');
                s.Append(item.Value.ToString());
                s.Append('\"');
            }
        }
        private static string MakeTagString(IEnumerable<KeyValuePair<string, object?>>? tags, ReadOnlySpan<KeyValuePair<string, object?>> otherTags)
        {
            var s = new StringBuilder();
            s.Append('{');
            if (tags != null)
            {
                var isFirst = true;
                foreach (var item in tags)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        s.Append(',');
                    }
                    AppendItem(s, item);
                }
                if (!otherTags.IsEmpty)
                {
                    for (int i = 0; i < otherTags.Length; i++)
                    {
                        var item = otherTags[i];
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            s.Append(',');
                        }
                        AppendItem(s, item);
                    }
                }
            }
            s.Append('}');
            return s.ToString();
        }

        public static bool TryGetCounterPayload(this EventWrittenEventArgs eventArgs, out ICounterPayload? payload)
        {
            if ("EventCounters".Equals(eventArgs.EventName))
            {
                IDictionary<string, object> payloadFields = (IDictionary<string, object>)eventArgs.Payload[0];

                //Make sure we are part of the requested series. If multiple clients request metrics, all of them get the metrics.
                string series = payloadFields["Series"].ToString();
                string counterName = payloadFields["Name"].ToString();

                string metadata = payloadFields["Metadata"].ToString();
                int seriesValue = TraceEventExtensions.GetInterval(series);

                float intervalSec = (float)payloadFields["IntervalSec"];
                string displayName = payloadFields["DisplayName"].ToString();
                string displayUnits = payloadFields["DisplayUnits"].ToString();
                double value = 0;
                CounterType counterType = CounterType.Metric;

                if (payloadFields["CounterType"].Equals("Mean"))
                {
                    value = (double)payloadFields["Mean"];
                }
                else if (payloadFields["CounterType"].Equals("Sum"))
                {
                    counterType = CounterType.Rate;
                    value = (double)payloadFields["Increment"];
                    if (string.IsNullOrEmpty(displayUnits))
                    {
                        displayUnits = "count";
                    }
                }

                payload = new EventCounterPayload(
#if NETSTANDARD2_0
                    DateTime.Now,
#else
                    eventArgs.TimeStamp,
#endif
                    eventArgs.EventName,
                    counterName, displayName,
                    displayUnits,
                    value,
                    counterType,
                    intervalSec,
                    seriesValue / 1000,
                    metadata,
                    null,
                    eventArgs);

                return true;
            }

            payload = null;
            return false;
        }

    }
    internal static class TraceEventExtensions
    {
        private static Dictionary<ProviderAndCounter, CounterMetadata> counterMetadataByName = new();
        private static HashSet<string> inactiveSharedSessions = new(StringComparer.OrdinalIgnoreCase);

        // This assumes uniqueness of provider/counter combinations;
        // this is currently a limitation (see https://github.com/dotnet/runtime/issues/93097 and https://github.com/dotnet/runtime/issues/93767)
        public static CounterMetadata GetCounterMetadata(string providerName, string counterName, string meterTags = null, string instrumentTags = null, string scopeHash = null)
        {
            ProviderAndCounter providerAndCounter = new(providerName, counterName);
            if (counterMetadataByName.TryGetValue(providerAndCounter, out CounterMetadata provider))
            {
                return provider;
            }

            counterMetadataByName.Add(providerAndCounter, new CounterMetadata(providerName, counterName, meterTags, instrumentTags, scopeHash));
            return counterMetadataByName[providerAndCounter];
        }
        public static bool TryGetCounterPayload(this TraceEvent traceEvent, CounterConfiguration counterConfiguration, out ICounterPayload payload)
        {
            payload = null;

            if ("EventCounters".Equals(traceEvent.EventName))
            {
                IDictionary<string, object> payloadVal = (IDictionary<string, object>)traceEvent.PayloadValue(0);
                IDictionary<string, object> payloadFields = (IDictionary<string, object>)payloadVal["Payload"];

                //Make sure we are part of the requested series. If multiple clients request metrics, all of them get the metrics.
                string series = payloadFields["Series"].ToString();
                string counterName = payloadFields["Name"].ToString();

                string metadata = payloadFields["Metadata"].ToString();
                int seriesValue = GetInterval(series);
                //CONSIDER
                //Concurrent counter sessions do not each get a separate interval. Instead the payload
                //for _all_ the counters changes the Series to be the lowest specified interval, on a per provider basis.
                //Currently the CounterFilter will remove any data whose Series doesn't match the requested interval.
                if (!counterConfiguration.CounterFilter.IsIncluded(traceEvent.ProviderName, counterName, seriesValue))
                {
                    return false;
                }

                float intervalSec = (float)payloadFields["IntervalSec"];
                string displayName = payloadFields["DisplayName"].ToString();
                string displayUnits = payloadFields["DisplayUnits"].ToString();
                double value = 0;
                CounterType counterType = CounterType.Metric;

                if (payloadFields["CounterType"].Equals("Mean"))
                {
                    value = (double)payloadFields["Mean"];
                }
                else if (payloadFields["CounterType"].Equals("Sum"))
                {
                    counterType = CounterType.Rate;
                    value = (double)payloadFields["Increment"];
                    if (string.IsNullOrEmpty(displayUnits))
                    {
                        displayUnits = "count";
                    }
                    //TODO Should we make these /sec like the dotnet-counters tool?
                }

                // Note that dimensional data such as pod and namespace are automatically added in prometheus and azure monitor scenarios.
                // We no longer added it here.

                payload = new EventCounterPayload(
                    traceEvent.TimeStamp,
                    traceEvent.ProviderName,
                    counterName, displayName,
                    displayUnits,
                    value,
                    counterType,
                    intervalSec,
                    seriesValue / 1000,
                    metadata,
                    traceEvent,
                    null);

                return true;
            }

            if (counterConfiguration.ClientId != null && !inactiveSharedSessions.Contains(counterConfiguration.ClientId) && MonitoringSourceConfiguration.SystemDiagnosticsMetricsProviderName.Equals(traceEvent.ProviderName))
            {
                if (traceEvent.EventName == "BeginInstrumentReporting")
                {
                    HandleBeginInstrumentReporting(traceEvent, counterConfiguration, out payload);
                }
                if (traceEvent.EventName == "HistogramValuePublished")
                {
                    HandleHistogram(traceEvent, counterConfiguration, out payload);
                }
                else if (traceEvent.EventName == "GaugeValuePublished")
                {
                    HandleGauge(traceEvent, counterConfiguration, out payload);
                }
                else if (traceEvent.EventName == "CounterRateValuePublished")
                {
                    HandleCounterRate(traceEvent, counterConfiguration, out payload);
                }
                else if (traceEvent.EventName == "UpDownCounterRateValuePublished")
                {
                    HandleUpDownCounterValue(traceEvent, counterConfiguration, out payload);
                }
                else if (traceEvent.EventName == "TimeSeriesLimitReached")
                {
                    HandleTimeSeriesLimitReached(traceEvent, counterConfiguration, out payload);
                }
                else if (traceEvent.EventName == "HistogramLimitReached")
                {
                    HandleHistogramLimitReached(traceEvent, counterConfiguration, out payload);
                }
                else if (traceEvent.EventName == "Error")
                {
                    HandleError(traceEvent, counterConfiguration, out payload);
                }
                else if (traceEvent.EventName == "ObservableInstrumentCallbackError")
                {
                    HandleObservableInstrumentCallbackError(traceEvent, counterConfiguration, out payload);
                }
                else if (traceEvent.EventName == "MultipleSessionsNotSupportedError")
                {
                    HandleMultipleSessionsNotSupportedError(traceEvent, counterConfiguration, out payload);
                }
                else if (traceEvent.EventName == "MultipleSessionsConfiguredIncorrectlyError")
                {
                    HandleMultipleSessionsConfiguredIncorrectlyError(traceEvent, counterConfiguration.ClientId, out payload);
                }

                return payload != null;
            }

            return false;
        }

        private static void HandleGauge(TraceEvent obj, CounterConfiguration counterConfiguration, out ICounterPayload payload)
        {
            payload = null;

            string payloadSessionId = (string)obj.PayloadValue(0);

            if (payloadSessionId != counterConfiguration.SessionId)
            {
                return;
            }

            string meterName = (string)obj.PayloadValue(1);
            //string meterVersion = (string)obj.PayloadValue(2);
            string instrumentName = (string)obj.PayloadValue(3);
            string unit = (string)obj.PayloadValue(4);
            string tags = (string)obj.PayloadValue(5);
            string lastValueText = (string)obj.PayloadValue(6);

            if (!counterConfiguration.CounterFilter.IsIncluded(meterName, instrumentName))
            {
                return;
            }

            // the value might be an empty string indicating no measurement was provided this collection interval
            if (double.TryParse(lastValueText, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out double lastValue))
            {
                payload = new GaugePayload(GetCounterMetadata(meterName, instrumentName), null, unit, tags, lastValue, obj.TimeStamp, obj, null);
            }
            else
            {
                // for observable instruments we assume the lack of data is meaningful and remove it from the UI
                // this happens when the Gauge callback function throws an exception.
                payload = new CounterEndedPayload(GetCounterMetadata(meterName, instrumentName), obj.TimeStamp, obj, null);
            }
        }

        private static void HandleBeginInstrumentReporting(TraceEvent traceEvent, CounterConfiguration counterConfiguration, out ICounterPayload payload)
        {
            payload = null;

            string payloadSessionId = (string)traceEvent.PayloadValue(0);
            if (payloadSessionId != counterConfiguration.SessionId)
            {
                return;
            }

            string meterName = (string)traceEvent.PayloadValue(1);
            //string meterVersion = (string)obj.PayloadValue(2);
            string instrumentName = (string)traceEvent.PayloadValue(3);

            if (!counterConfiguration.CounterFilter.IsIncluded(meterName, instrumentName))
            {
                return;
            }

            if (traceEvent.Version < 1)
            {
                payload = new BeginInstrumentReportingPayload(GetCounterMetadata(meterName, instrumentName), traceEvent.TimeStamp, traceEvent, null);
            }
            else
            {
                string instrumentTags = (string)traceEvent.PayloadValue(7);
                string meterTags = (string)traceEvent.PayloadValue(8);
                string meterScopeHash = (string)traceEvent.PayloadValue(9);

                payload = new BeginInstrumentReportingPayload(GetCounterMetadata(meterName, instrumentName, meterTags, instrumentTags, meterScopeHash), traceEvent.TimeStamp, traceEvent, null);
            }
        }

        private static void HandleCounterRate(TraceEvent traceEvent, CounterConfiguration counterConfiguration, out ICounterPayload payload)
        {
            payload = null;

            string payloadSessionId = (string)traceEvent.PayloadValue(0);

            if (payloadSessionId != counterConfiguration.SessionId)
            {
                return;
            }

            string meterName = (string)traceEvent.PayloadValue(1);
            //string meterVersion = (string)obj.PayloadValue(2);
            string instrumentName = (string)traceEvent.PayloadValue(3);
            string unit = (string)traceEvent.PayloadValue(4);
            string tags = (string)traceEvent.PayloadValue(5);
            string rateText = (string)traceEvent.PayloadValue(6);

            if (!counterConfiguration.CounterFilter.IsIncluded(meterName, instrumentName))
            {
                return;
            }

            if (double.TryParse(rateText, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                payload = new RatePayload(GetCounterMetadata(meterName, instrumentName), null, unit, tags, value, counterConfiguration.CounterFilter.DefaultIntervalSeconds, traceEvent.TimeStamp, traceEvent, null);
            }
            else
            {
                // for observable instruments we assume the lack of data is meaningful and remove it from the UI
                // this happens when the ObservableCounter callback function throws an exception
                // or when the ObservableCounter doesn't include a measurement for a particular set of tag values.
                payload = new CounterEndedPayload(GetCounterMetadata(meterName, instrumentName), traceEvent.TimeStamp, traceEvent, null);
            }
        }

        private static void HandleUpDownCounterValue(TraceEvent traceEvent, CounterConfiguration configuration, out ICounterPayload payload)
        {
            payload = null;

            string payloadSessionId = (string)traceEvent.PayloadValue(0);

            if (payloadSessionId != configuration.SessionId || traceEvent.Version < 1) // Version 1 added the value field.
            {
                return;
            }

            string meterName = (string)traceEvent.PayloadValue(1);
            //string meterVersion = (string)obj.PayloadValue(2);
            string instrumentName = (string)traceEvent.PayloadValue(3);
            string unit = (string)traceEvent.PayloadValue(4);
            string tags = (string)traceEvent.PayloadValue(5);
            //string rateText = (string)traceEvent.PayloadValue(6); // Not currently using rate for UpDownCounters.
            string valueText = (string)traceEvent.PayloadValue(7);

            if (!configuration.CounterFilter.IsIncluded(meterName, instrumentName))
            {
                return;
            }

            if (double.TryParse(valueText, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                // UpDownCounter reports the value, not the rate - this is different than how Counter behaves.
                payload = new UpDownCounterPayload(GetCounterMetadata(meterName, instrumentName), null, unit, tags, value, traceEvent.TimeStamp, traceEvent, null);

            }
            else
            {
                // for observable instruments we assume the lack of data is meaningful and remove it from the UI
                // this happens when the ObservableUpDownCounter callback function throws an exception
                // or when the ObservableUpDownCounter doesn't include a measurement for a particular set of tag values.
                payload = new CounterEndedPayload(GetCounterMetadata(meterName, instrumentName), traceEvent.TimeStamp, traceEvent, null);
            }
        }

        private static void HandleHistogram(TraceEvent obj, CounterConfiguration configuration, out ICounterPayload payload)
        {
            payload = null;

            string payloadSessionId = (string)obj.PayloadValue(0);

            if (payloadSessionId != configuration.SessionId)
            {
                return;
            }

            string meterName = (string)obj.PayloadValue(1);
            //string meterVersion = (string)obj.PayloadValue(2);
            string instrumentName = (string)obj.PayloadValue(3);
            string unit = (string)obj.PayloadValue(4);
            string tags = (string)obj.PayloadValue(5);
            string quantilesText = (string)obj.PayloadValue(6);

            if (!configuration.CounterFilter.IsIncluded(meterName, instrumentName))
            {
                return;
            }

            //Note quantiles can be empty.
            IList<Quantile> quantiles = ParseQuantiles(quantilesText);

            payload = new AggregatePercentilePayload(GetCounterMetadata(meterName, instrumentName), null, unit, tags, quantiles, obj.TimeStamp, obj, null);
        }

        private static void HandleHistogramLimitReached(TraceEvent obj, CounterConfiguration configuration, out ICounterPayload payload)
        {
            payload = null;

            string payloadSessionId = (string)obj.PayloadValue(0);

            if (payloadSessionId != configuration.SessionId)
            {
                return;
            }

            string errorMessage = $"Warning: Histogram tracking limit ({configuration.MaxHistograms}) reached. Not all data is being shown. The limit can be changed but will use more memory in the target process.";

            payload = new ErrorPayload(errorMessage, obj.TimeStamp, EventType.HistogramLimitError, obj, null);
        }

        private static void HandleTimeSeriesLimitReached(TraceEvent obj, CounterConfiguration configuration, out ICounterPayload payload)
        {
            payload = null;

            string payloadSessionId = (string)obj.PayloadValue(0);

            if (payloadSessionId != configuration.SessionId)
            {
                return;
            }

            string errorMessage = $"Warning: Time series tracking limit ({configuration.MaxTimeseries}) reached. Not all data is being shown. The limit can be changed but will use more memory in the target process.";

            payload = new ErrorPayload(errorMessage, obj.TimeStamp, EventType.TimeSeriesLimitError, obj, null);
        }

        private static void HandleError(TraceEvent obj, CounterConfiguration configuration, out ICounterPayload payload)
        {
            payload = null;

            string payloadSessionId = (string)obj.PayloadValue(0);
            string error = (string)obj.PayloadValue(1);
            if (configuration.SessionId != payloadSessionId)
            {
                return;
            }

            string errorMessage = "Error reported from target process:" + Environment.NewLine + error;

            payload = new ErrorPayload(errorMessage, obj.TimeStamp, EventType.ErrorTargetProcess, obj, null);
        }

        private static void HandleMultipleSessionsNotSupportedError(TraceEvent obj, CounterConfiguration configuration, out ICounterPayload payload)
        {
            payload = null;

            string payloadSessionId = (string)obj.PayloadValue(0);
            if (payloadSessionId == configuration.SessionId)
            {
                // If our session is the one that is running then the error is not for us,
                // it is for some other session that came later
                return;
            }
            else
            {
                string errorMessage = "Error: Another metrics collection session is already in progress for the target process." + Environment.NewLine +
                "Concurrent sessions are not supported.";

                payload = new ErrorPayload(errorMessage, obj.TimeStamp, EventType.MultipleSessionsNotSupportedError, obj, null);
            }
        }

        internal static bool TryCreateSharedSessionConfiguredIncorrectlyMessage(TraceEvent obj, string clientId, out string message)
        {
            message = string.Empty;

            string payloadSessionId = (string)obj.PayloadValue(0);

            if (payloadSessionId != clientId)
            {
                // If our session is not the one that is running then the error is not for us,
                // it is for some other session that came later
                return false;
            }

            string expectedMaxHistograms = (string)obj.PayloadValue(1);
            string actualMaxHistograms = (string)obj.PayloadValue(2);
            string expectedMaxTimeSeries = (string)obj.PayloadValue(3);
            string actualMaxTimeSeries = (string)obj.PayloadValue(4);
            string expectedRefreshInterval = (string)obj.PayloadValue(5);
            string actualRefreshInterval = (string)obj.PayloadValue(6);

            StringBuilder errorMessage = new("Error: Another shared metrics collection session is already in progress for the target process." + Environment.NewLine +
            "To enable this metrics session alongside the existing session, update the following values:" + Environment.NewLine);

            if (expectedMaxHistograms != actualMaxHistograms)
            {
                errorMessage.Append($"MaxHistograms: {expectedMaxHistograms}" + Environment.NewLine);
            }
            if (expectedMaxTimeSeries != actualMaxTimeSeries)
            {
                errorMessage.Append($"MaxTimeSeries: {expectedMaxTimeSeries}" + Environment.NewLine);
            }
            if (expectedRefreshInterval != actualRefreshInterval)
            {
                errorMessage.Append($"IntervalSeconds: {expectedRefreshInterval}" + Environment.NewLine);
            }

            message = errorMessage.ToString();

            return true;
        }

        private static void HandleMultipleSessionsConfiguredIncorrectlyError(TraceEvent obj, string clientId, out ICounterPayload payload)
        {
            payload = null;

            if (TryCreateSharedSessionConfiguredIncorrectlyMessage(obj, clientId, out string message))
            {
                payload = new ErrorPayload(message.ToString(), obj.TimeStamp, EventType.MultipleSessionsConfiguredIncorrectlyError, obj, null);

                inactiveSharedSessions.Add(clientId);
            }
        }

        private static void HandleObservableInstrumentCallbackError(TraceEvent obj, CounterConfiguration configuration, out ICounterPayload payload)
        {
            payload = null;

            string payloadSessionId = (string)obj.PayloadValue(0);
            string error = (string)obj.PayloadValue(1);

            if (payloadSessionId != configuration.SessionId)
            {
                return;
            }

            string errorMessage = "Exception thrown from an observable instrument callback in the target process:" + Environment.NewLine +
                error;

            payload = new ErrorPayload(errorMessage, obj.TimeStamp, EventType.ObservableInstrumentCallbackError, obj, null);
        }
        private static List<Quantile> ParseQuantiles(string quantileList)
        {
            string[] quantileParts = quantileList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<Quantile> quantiles = new();
            foreach (string quantile in quantileParts)
            {
                string[] keyValParts = quantile.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (keyValParts.Length != 2)
                {
                    continue;
                }
                if (!double.TryParse(keyValParts[0], NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out double key))
                {
                    continue;
                }
                if (!double.TryParse(keyValParts[1], NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                {
                    continue;
                }
                quantiles.Add(new Quantile(key, val));
            }
            return quantiles;
        }

        internal static int GetInterval(string series)
        {
            const string comparison = "Interval=";
            int interval = 0;
            if (series.StartsWith(comparison, StringComparison.OrdinalIgnoreCase))
            {
#if NETSTANDARD2_0
                int.TryParse(series.AsSpan(comparison.Length).ToString(), out interval);
#else
                int.TryParse(series.AsSpan(comparison.Length), out interval);
#endif
            }
            return interval;
        }
    }
}
