using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ValueBuffer;

namespace Diagnostics.Traces.DuckDB
{
    internal static class DuckHelper
    {
        public static unsafe void WrapValue<T>(ref ValueStringBuilder builder,T? input)
        {
            if (input ==null || DBNull.Value.Equals(input))
            {
                builder.Append("NULL");
                return;
            }
            else if (input is string || input is Guid||input is Exception)
            {
                var str = input.ToString();
                var sp = str.AsSpan();
                if (!sp.IsEmpty && sp.IndexOf('\'') != -1)
                {
                    str = str!.Replace("'", "''");
                }
                builder.Append('\'');
                builder.Append(str);
                builder.Append('\'');
                return;
            }
            else if (input is DateTime dt)
            {
                if (dt.Date == dt)
                {
                    builder.AppendDate(dt);
                    return;
                }
                builder.Append('\'');
                builder.AppendDateTime(dt);
                builder.Append('.');
                builder.Append(dt.Millisecond.ToString());
                builder.Append('\'');
                return;
            }
            else if (input is DateTimeOffset timeOffset)
            {
                builder.Append('\'');
                builder.Append(timeOffset.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                builder.Append('\'');
                return;
            }
            else if (input is byte[] buffer)
            {
                builder.Append("0x");
                builder.Append(BitConverter.ToString(buffer).Replace("-", string.Empty));
                return;
            }
            else if (input is IEnumerable<KeyValuePair<string, object?>> arrayObject)
            {
                MapAsString(ref builder, arrayObject);
                return;
            }
            else if (input is IEnumerable<KeyValuePair<string, string?>> arrayString)
            {
                MapAsString(ref builder, arrayString);
                return;
            }
            else if (input is IEnumerable<ActivityEvent> events)
            {
                MapAsString(ref builder, events);
                return;
            }
            else if (input is IEnumerable<ActivityLink> links)
            {
                MapAsString(ref builder, links);
                return;
            }
            else if (input is ActivityContext context)
            {
                MapAsString(ref builder, context);
                return;
            }
            else if (input is ReadOnlyTagCollection tags)
            {
                MapAsString(ref builder, tags);
                return;
            }
            else if (input is IDictionary dictionary)
            {
                MapAsString(ref builder, dictionary);
                return;
            }
            else if (input is bool b)
            {
                if (b)
                {
                    builder.Append("true");
                }
                else
                {
                    builder.Append("false");
                }
                return;
            }
            else if (input is double d)
            {                
                if (double.IsPositiveInfinity(d))
                {
                    builder.Append("'Infinity'::DOUBLE");
                    return;
                }
                else if (double.IsNegativeInfinity(d))
                {
                    builder.Append("'-Infinity'::DOUBLE");
                    return;
                }
                else if (double.IsNaN(d))
                {
                    builder.Append("'NaN'::DOUBLE");
                    return;
                }
#if !NETSTANDARD2_0
                Span<char> doubleBuffer = stackalloc char[256];
                if (d.TryFormat(doubleBuffer,out var written))
                {
                    builder.Append(doubleBuffer.Slice(0, written));
                    return;
                }
#endif
            }
            else if (input is float f)
            {
                if (float.IsPositiveInfinity(f))
                {
                    builder.Append("'Infinity'::FLOAT");
                    return;
                }
                else if (float.IsNegativeInfinity(f))
                {
                    builder.Append("'-Infinity'::FLOAT");
                    return;
                }
                else if (float.IsNaN(f))
                {
                    builder.Append("'-NaN'::FLOAT");
                    return;
                }
#if !NETSTANDARD2_0
                Span<char> numBuffer = stackalloc char[256];
                if (f.TryFormat(numBuffer, out var written))
                {
                    builder.Append(numBuffer.Slice(0, written));
                    return;
                }
#endif
            }
#if !NETSTANDARD2_0
            else if (input is int i)
            {
                Span<char> numBuffer = stackalloc char[11];
                if (i.TryFormat(numBuffer, out var written))
                {
                    builder.Append(numBuffer.Slice(0, written));
                    return;
                }
            }
            else if (input is long l)
            {
                Span<char> numBuffer = stackalloc char[20];
                if (l.TryFormat(numBuffer, out var written))
                {
                    builder.Append(numBuffer.Slice(0, written));
                    return;
                }
            }
            else if (input is short s)
            {
                Span<char> numBuffer = stackalloc char[6];
                if (s.TryFormat(numBuffer, out var written))
                {
                    builder.Append(numBuffer.Slice(0, written));
                    return;
                }
            }
#endif
            else if (input is Enum e)
            {
                builder.Append(Enum.Format(TypeCache<T>.type, e, "D"));
                return;
            }
            builder.Append(input?.ToString()!);
        }
        static class TypeCache<T>
        {
            public static readonly Type type = typeof(T);
        }
        private static void MapAsString(ref ValueStringBuilder s, MetricType metricType, in MetricPoint point)
        {
            s.Append("{");
            if (metricType == MetricType.Histogram || metricType == MetricType.ExponentialHistogram)
            {
                s.Append("'value':NULL,'sum':");
                WrapValue(ref s, point.GetHistogramSum());
                s.Append(",'count':");
                WrapValue(ref s, point.GetHistogramCount());
                s.Append(',');
                if (point.TryGetHistogramMinMaxValues(out double min, out double max))
                {
                    s.Append("'min':");
                    WrapValue(ref s, min);
                    s.Append(",'max':");
                    WrapValue(ref s, max);
                    s.Append(',');
                }
                else
                {

                    s.Append("'min':NULL,'max':NULL,");
                }
                s.Append("'histogram':ARRAY [");
                if (metricType == MetricType.Histogram)
                {
                    var isFirstIteration = true;
                    var previousExplicitBound = 0d;
                    foreach (var histogramMeasurement in point.GetHistogramBuckets())
                    {
                        if (!isFirstIteration)
                        {
                            s.Append(',');
                        }
                        s.Append("{");
                        if (isFirstIteration)
                        {
                            s.Append("'rangeLeft':");
                            WrapValue(ref s, double.NegativeInfinity);
                            s.Append(",'rangeRight':");
                            WrapValue(ref s, histogramMeasurement.ExplicitBound);

                            s.Append(",'bucketCount':");
                            WrapValue(ref s,histogramMeasurement.BucketCount);
                            previousExplicitBound = histogramMeasurement.ExplicitBound;
                            isFirstIteration = false;
                        }
                        else
                        {
                            s.Append("'rangeLeft':");
                            WrapValue(ref s,previousExplicitBound);
                            s.Append(",'rangeRight':");

                            if (histogramMeasurement.ExplicitBound != double.PositiveInfinity)
                            {
                                WrapValue(ref s,histogramMeasurement.ExplicitBound);
                                previousExplicitBound = histogramMeasurement.ExplicitBound;
                            }
                            else
                            {
                                WrapValue(ref s, double.PositiveInfinity);
                            }

                            s.Append(",'bucketCount':");
                            WrapValue(ref s,histogramMeasurement.BucketCount);
                        }
                        s.Append("}");
                    }
                    //s.Remove(s.Length - 1, 1);
                }
                else
                {
                    var exponentialHistogramData = point.GetExponentialHistogramData();
                    s.Append("'histogram':NULL,zeroCount':");
                    WrapValue(ref s,exponentialHistogramData.ZeroCount);
                    s.Append("'buckets':ARRAY [");

                    var scale = exponentialHistogramData.Scale;
                    var offset = exponentialHistogramData.PositiveBuckets.Offset;
                    var isFirst = true;

                    foreach (var bucketCount in exponentialHistogramData.PositiveBuckets)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            s.Append(',');
                        }
                        s.Append("{");
                        s.Append("\"lowerBound\":");
                        WrapValue(ref s,Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale));
                        s.Append(",\"upperBound\":");
                        WrapValue(ref s, Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale));
                        s.Append(",\"bucketCount\":");
                        WrapValue(ref s, bucketCount);
                        s.Append("}");
                    }
                    s.Append("]");
                    //s.Remove(s.Length - 1, 1);
                }
                s.Append("],'zeroBucketCount':NULL,'buckets':NULL,");
            }
            else
            {
                s.Append("'value':");
                if (metricType.IsDouble())
                {
                    if (metricType.IsSum())
                    {
                        WrapValue(ref s, point.GetSumDouble());
                    }
                    else
                    {
                        WrapValue(ref s, point.GetGaugeLastValueDouble());
                    }
                }
                else if (metricType.IsLong())
                {
                    if (metricType.IsSum())
                    {
                        WrapValue(ref s, point.GetSumLong());
                    }
                    else
                    {
                        WrapValue(ref s, point.GetGaugeLastValueLong());
                    }
                }
                s.Append(",'sum':NULL,'min':NULL,'max':NULL,'count':NULL,'histogram':NULL,'zeroBucketCount':NULL,'buckets':NULL,");
            }
            s.Append("'startTime':");
            WrapValue(ref s, point.StartTime);
            s.Append(",'endTime':");
            WrapValue(ref s, point.StartTime);
            s.Append(",'tags':");
            WrapValue(ref s, point.Tags);
            s.Append("}");
        }
        internal static void MapAsString(ref ValueStringBuilder s, MetricType metricType, in MetricPointsAccessor points)
        {
            s.Append("ARRAY [");
            var isFirst = true;
            foreach (ref readonly var item in points)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    s.Append(',');
                }
                MapAsString(ref s, metricType, item);
            }
            s.Append(']');
        }
        private static void MapAsString(ref ValueStringBuilder builder, in ActivityContext context)
        {
            builder.Append("{'traceId':");
            WrapValue(ref builder,context.TraceId.ToString());
            builder.Append(",'traceState':");
            WrapValue(ref builder, context.TraceState);
            builder.Append(",'traceFlags':");
            WrapValue(ref builder, context.TraceFlags);
            builder.Append(",'isRemote':");
            WrapValue(ref builder, context.IsRemote);
            builder.Append(",'spanId':");
            WrapValue(ref builder, context.SpanId.ToString());
            builder.Append('}');
        }

        private static void MapAsString(ref ValueStringBuilder builder, IEnumerable<ActivityLink> links)
        {
            if (!links.Any())
            {
                builder.Append("ARRAY []");
                return;
            }

            var isFirst = true;
            builder.Append("ARRAY [");
            foreach (var item in links)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    builder.Append(',');
                }
                builder.Append("{'context':");
                WrapValue(ref builder,item.Context);
                builder.Append(",'tags':");
                WrapValue(ref builder,item.Tags);
                builder.Append("}");
            }

            builder.Append(']');

        }
        private static void MapAsString(ref ValueStringBuilder builder, IEnumerable<ActivityEvent> events)
        {
            if (!events.Any())
            {
                builder.Append("ARRAY []");
                return;
            }

            var isFirst = true;
            builder.Append("ARRAY [");
            foreach (var item in events)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    builder.Append(',');
                }
                builder.Append("{'name':");
                WrapValue(ref builder,item.Name);
                builder.Append(",'timestamp':");
                WrapValue(ref builder, item.Timestamp);
                builder.Append(",'tags':");
                WrapValue(ref builder, item.Tags);
                builder.Append("}");
            }
            builder.Append(']');
        }
        private static void MapAsString(ref ValueStringBuilder builder, in ReadOnlyTagCollection tags)
        {
            if (tags.Count == 0)
            {
                builder.Append("MAP {}");
                return;
            }
            var isFirst = true;
            var addedSet = new HashSet<string>();
            builder.Append("MAP {");
            foreach (var item in tags)
            {
                if (!addedSet.Add(item.Key))
                {
                    continue;
                }
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    builder.Append(',');
                }
                WrapValue(ref builder, item.Key);
                builder.Append(':');
                WrapValue(ref builder, item.Value);
            }
            //s.Remove(s.Length - 1, 1);
            builder.Append('}');
        }
        private static void MapAsString<T>(ref ValueStringBuilder builder, IEnumerable<KeyValuePair<string, T>> arrayObject)
        {
            if (!arrayObject.Any())
            {
                builder.Append("MAP {}");
                return;
            }
            var isFirst = true;
            var addedSet = new HashSet<string>();
            builder.Append("MAP {");
            foreach (var item in arrayObject)
            {
                if (!addedSet.Add(item.Key))
                {
                    continue;
                }
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    builder.Append(',');
                }
                WrapValue(ref builder, item.Key);
                builder.Append(':');
                WrapValue(ref builder, item.Value);
            }
            builder.Append('}');
        }
        private static void MapAsString(ref ValueStringBuilder builder, IDictionary arrayObject)
        {
            if (arrayObject.Count == 0)
            {
                builder.Append("MAP {}");
                return;
            }
            var isFirst = true;
            var addedSet=new HashSet<string>();
            builder.Append("MAP {");
            foreach (KeyValuePair<object, object?> item in arrayObject)
            {
                if (item.Key == null)
                {
                    continue;
                }
                var key = item.Key.ToString();
                if (key != null && !addedSet.Add(key))
                {
                    continue;
                }
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    builder.Append(',');
                }
                WrapValue(ref builder, key);
                builder.Append(':');
                WrapValue(ref builder, item.Value);
            }
            builder.Append('}');
        }
    }
}
