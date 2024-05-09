using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Diagnostics.Traces.Zips
{
    //internal static class MetricExportHelper
    //{
    //    public static void A(StringBuilder msg,Metric metric)
    //    {
    //        var unit = string.IsNullOrEmpty(metric.Unit) ? string.Empty : $"({metric.Unit})";
    //        var metricName = string.IsNullOrEmpty(metric.MeterName) ? string.Empty : metric.MeterName;
    //        var metricVersion = string.IsNullOrEmpty(metric.MeterVersion) ? string.Empty : metric.MeterVersion;
    //        var metricInfo = string.Empty;
    //        if (!string.IsNullOrEmpty(metricName))
    //        {
    //            metricInfo = $"[{metricName}:{metricVersion}]";
    //        }
    //        var tags = string.Empty;
    //        if (metric.MeterTags!=null&&metric.MeterTags.Any())
    //        {
    //            tags = $"{{ {string.Join(", ", metric.MeterTags.Select(x =>$"{x.Key}:{x.Value}"))} }}";
    //        }
    //        msg.Append("[[ ");
    //        msg.AppendLine($"{metric.Name}{metricInfo} {tags}");


    //        foreach (ref readonly var metricPoint in metric.GetMetricPoints())
    //        {
    //            string valueDisplay = string.Empty;
    //            foreach (var tag in metricPoint.Tags)
    //            {
    //                if (this.TagTransformer.TryTransformTag(tag, out var result))
    //                {
    //                    tagsBuilder.Append(result);
    //                    tagsBuilder.Append(' ');
    //                }
    //            }

    //            var tags = tagsBuilder.ToString().TrimEnd();

    //            var metricType = metric.MetricType;

    //            if (metricType == MetricType.Histogram || metricType == MetricType.ExponentialHistogram)
    //            {
    //                var bucketsBuilder = new StringBuilder();
    //                var sum = metricPoint.GetHistogramSum();
    //                var count = metricPoint.GetHistogramCount();
    //                bucketsBuilder.Append($"Sum: {sum} Count: {count} ");
    //                if (metricPoint.TryGetHistogramMinMaxValues(out double min, out double max))
    //                {
    //                    bucketsBuilder.Append($"Min: {min} Max: {max} ");
    //                }

    //                bucketsBuilder.AppendLine();

    //                if (metricType == MetricType.Histogram)
    //                {
    //                    bool isFirstIteration = true;
    //                    double previousExplicitBound = default;
    //                    foreach (var histogramMeasurement in metricPoint.GetHistogramBuckets())
    //                    {
    //                        if (isFirstIteration)
    //                        {
    //                            bucketsBuilder.Append("(-Infinity,");
    //                            bucketsBuilder.Append(histogramMeasurement.ExplicitBound);
    //                            bucketsBuilder.Append(']');
    //                            bucketsBuilder.Append(':');
    //                            bucketsBuilder.Append(histogramMeasurement.BucketCount);
    //                            previousExplicitBound = histogramMeasurement.ExplicitBound;
    //                            isFirstIteration = false;
    //                        }
    //                        else
    //                        {
    //                            bucketsBuilder.Append('(');
    //                            bucketsBuilder.Append(previousExplicitBound);
    //                            bucketsBuilder.Append(',');
    //                            if (histogramMeasurement.ExplicitBound != double.PositiveInfinity)
    //                            {
    //                                bucketsBuilder.Append(histogramMeasurement.ExplicitBound);
    //                                previousExplicitBound = histogramMeasurement.ExplicitBound;
    //                            }
    //                            else
    //                            {
    //                                bucketsBuilder.Append("+Infinity");
    //                            }

    //                            bucketsBuilder.Append(']');
    //                            bucketsBuilder.Append(':');
    //                            bucketsBuilder.Append(histogramMeasurement.BucketCount);
    //                        }

    //                        bucketsBuilder.AppendLine();
    //                    }
    //                }
    //                else
    //                {
    //                    var exponentialHistogramData = metricPoint.GetExponentialHistogramData();
    //                    var scale = exponentialHistogramData.Scale;

    //                    if (exponentialHistogramData.ZeroCount != 0)
    //                    {
    //                        bucketsBuilder.AppendLine($"Zero Bucket:{exponentialHistogramData.ZeroCount}");
    //                    }

    //                    var offset = exponentialHistogramData.PositiveBuckets.Offset;
    //                    foreach (var bucketCount in exponentialHistogramData.PositiveBuckets)
    //                    {
    //                        var lowerBound = Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale).ToString(CultureInfo.InvariantCulture);
    //                        var upperBound = Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(++offset, scale).ToString(CultureInfo.InvariantCulture);
    //                        bucketsBuilder.AppendLine($"({lowerBound}, {upperBound}]:{bucketCount}");
    //                    }
    //                }

    //                valueDisplay = bucketsBuilder.ToString();
    //            }
    //            else if (metricType.IsDouble())
    //            {
    //                if (metricType.IsSum())
    //                {
    //                    valueDisplay = metricPoint.GetSumDouble().ToString(CultureInfo.InvariantCulture);
    //                }
    //                else
    //                {
    //                    valueDisplay = metricPoint.GetGaugeLastValueDouble().ToString(CultureInfo.InvariantCulture);
    //                }
    //            }
    //            else if (metricType.IsLong())
    //            {
    //                if (metricType.IsSum())
    //                {
    //                    valueDisplay = metricPoint.GetSumLong().ToString(CultureInfo.InvariantCulture);
    //                }
    //                else
    //                {
    //                    valueDisplay = metricPoint.GetGaugeLastValueLong().ToString(CultureInfo.InvariantCulture);
    //                }
    //            }

    //            var exemplarString = new StringBuilder();
    //            if (metricPoint.TryGetExemplars(out var exemplars))
    //            {
    //                foreach (ref readonly var exemplar in exemplars)
    //                {
    //                    exemplarString.Append("Timestamp: ");
    //                    exemplarString.Append(exemplar.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture));
    //                    if (metricType.IsDouble())
    //                    {
    //                        exemplarString.Append(" Value: ");
    //                        exemplarString.Append(exemplar.DoubleValue);
    //                    }
    //                    else if (metricType.IsLong())
    //                    {
    //                        exemplarString.Append(" Value: ");
    //                        exemplarString.Append(exemplar.LongValue);
    //                    }

    //                    if (exemplar.TraceId != default)
    //                    {
    //                        exemplarString.Append(" TraceId: ");
    //                        exemplarString.Append(exemplar.TraceId.ToHexString());
    //                        exemplarString.Append(" SpanId: ");
    //                        exemplarString.Append(exemplar.SpanId.ToHexString());
    //                    }

    //                    bool appendedTagString = false;
    //                    foreach (var tag in exemplar.FilteredTags)
    //                    {
    //                        if (this.TagTransformer.TryTransformTag(tag, out var result))
    //                        {
    //                            if (!appendedTagString)
    //                            {
    //                                exemplarString.Append(" Filtered Tags : ");
    //                                appendedTagString = true;
    //                            }

    //                            exemplarString.Append(result);
    //                            exemplarString.Append(' ');
    //                        }
    //                    }

    //                    exemplarString.AppendLine();
    //                }
    //            }

    //            msg.Append('(');
    //            msg.Append(metricPoint.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture));
    //            msg.Append(", ");
    //            msg.Append(metricPoint.EndTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture));
    //            msg.Append("] ");
    //            msg.Append(tags);
    //            if (tags != string.Empty)
    //            {
    //                msg.Append(' ');
    //            }

    //            msg.Append(metric.MetricType);
    //            msg.AppendLine();
    //            msg.Append($"Value: {valueDisplay}");

    //            if (exemplarString.Length > 0)
    //            {
    //                msg.AppendLine();
    //                msg.AppendLine("Exemplars");
    //                msg.Append(exemplarString.ToString());
    //            }

    //        }

    //        builder.AppendLine("]]");
    //    }
    //}

    public delegate Task ZipTraceHandler<TIdentity, TInput>(TIdentity identity, TInput input, ZipTraceEntry entry, CancellationToken token);
    public delegate Task ZipTraceStreamHandler<TIdentity, TInput>(TIdentity identity, TInput input, Stream stream, CancellationToken token);

    public class ZipTraceHandler<TIdentity> : IActivityTraceHandler, ILogRecordTraceHandler, IMetricTraceHandler, IDisposable
        where TIdentity : IEquatable<TIdentity>
    {
        public const string ActivityEntryName = "activity";
        public const string LogEntryName = "log";
        public const string MetricEntryName = "metric";

        private static readonly byte[] newLineBuffer=Encoding.UTF8.GetBytes(Environment.NewLine);

        public ZipTraceHandler(ZipTraceManager<TIdentity> zipTraceManager, 
            IPhysicalPathProvider<TIdentity> physicalPathProvider,
            IIdentityProvider<TIdentity, Activity> activityIdentityProvider,
            IIdentityProvider<TIdentity, LogRecord> logIdentityProvider,
            IIdentityProvider<TIdentity, Metric> metricIdentityProvider)
        {
            ZipTraceManager = zipTraceManager;
            PhysicalPathProvider = physicalPathProvider;
            ActivityIdentityProvider = activityIdentityProvider;
            LogIdentityProvider = logIdentityProvider;
            MetricIdentityProvider = metricIdentityProvider;
        }

        public ZipTraceManager<TIdentity> ZipTraceManager { get; }

        public IPhysicalPathProvider<TIdentity> PhysicalPathProvider { get; }

        public IIdentityProvider<TIdentity,Activity> ActivityIdentityProvider { get; }

        public IIdentityProvider<TIdentity,LogRecord> LogIdentityProvider { get; }

        public IIdentityProvider<TIdentity,Metric> MetricIdentityProvider { get; }

        protected async Task HandleCoreAsync<TInput>(IIdentityProvider<TIdentity, TInput> provider, TInput input, ZipTraceHandler<TIdentity, TInput> handle, CancellationToken token)
        {
            if (!provider.HasIdentity(input))
            {
                return;
            }
            var identity = provider.GetIdentity(input);

            var entity = ZipTraceManager.GetOrAdd(identity, k => new ZipTraceEntry(PhysicalPathProvider.GetPath(k)));

            await entity.Slim.WaitAsync(token);
            try
            {
                await handle(identity, input, entity, token);
            }
            finally
            {
                entity.Slim.Release();
            }
        }
        protected Task HandleWithStreamEndAsync<TInput>(IIdentityProvider<TIdentity, TInput> provider, TInput input, ZipTraceStreamHandler<TIdentity, TInput> handle,string entryName, CancellationToken token)
        {
            return HandleCoreAsync(provider, input, async (identity, activity, entity,token) =>
            {
                using (var stream = entity.GetOrCreateOpenStream(entryName))
                {
                    stream.Seek(0, SeekOrigin.End);

                    await handle(identity, activity, stream, token);
                }
            }, token);
        }

        public Task HandleAsync(Activity input, CancellationToken token)
        {
            return HandleWithStreamEndAsync(ActivityIdentityProvider, input, async (identity, activity, stream, token) =>
            {
                using (var utf8Writer = new Utf8JsonWriter(stream, new JsonWriterOptions
                {
                    SkipValidation =
#if DEBUG
                        false
#else
                        true
#endif
                }))
                {
                    ActivityJsonConverter.Write(utf8Writer, input);
                }
                await stream.WriteAsync(newLineBuffer,0, newLineBuffer.Length,token);
            },input.TraceId.ToString(), token);
        }

        public Task HandleAsync(LogRecord input, CancellationToken token)
        {
            return HandleWithStreamEndAsync(LogIdentityProvider, input, async (identity, log, stream, token) =>
            {
                var str = $"{log.Timestamp.ToLocalTime():yyyy-MM-dd HH:mm:ss.ffff},{log.LogLevel},{log.CategoryName},{log.TraceId},{log.SpanId},{log.State}";
                await stream.WriteStringAsync(str, token);
                await stream.WriteAsync(newLineBuffer, 0, newLineBuffer.Length, token);
            },LogEntryName, token);
        }

        public Task HandleAsync(Metric input, CancellationToken token)
        {
            return HandleWithStreamEndAsync(MetricIdentityProvider, input, async (identity, metric, stream, token) =>
            {
                //var str = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{metric.Name},{metric.MeterName},{metric.MeterVersion},{metric.}";
                //await stream.WriteStringAsync(str, token);
            },MetricEntryName, token);
        }

        public void Dispose()
        {
            ZipTraceManager.Dispose();
        }
    }
}
