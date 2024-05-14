using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using ValueBuffer;

namespace Diagnostics.Traces.DuckDB
{
    internal static class DuckHelper
    {
        public static unsafe string WrapValue<T>(T? input)
        {
            if (input == null || Equals(input, DBNull.Value))
            {
                return "NULL";
            }
            else if (input is string || input is Guid)
            {
                var str = input.ToString();
                if (!string.IsNullOrEmpty(str) && str.AsSpan().IndexOf('\'') != -1)
                {
                    str = str.Replace("'", "''");
                }
                return $"'{str}'";
            }
            else if (input is DateTime dt)
            {
                if (dt.Date == dt)
                {
                    return $"'{dt:yyyy-MM-dd}'";
                }
                return $"'{dt:yyyy-MM-dd HH:mm:ss.ffff}'";
            }
            else if (input is DateTimeOffset timeOffset)
            {
                return $"'{timeOffset:yyyy-MM-dd HH:mm:ss.ffff}'";
            }
            else if (input is byte[] buffer)
            {
                return $"0x{BitConverter.ToString(buffer).Replace("-", string.Empty)}";
            }
            else if (input is IEnumerable<KeyValuePair<string, object?>> arrayObject)
            {
                return MapAsString(arrayObject);
            }
            else if (input is IEnumerable<KeyValuePair<string, string?>> arrayString)
            {
                return MapAsString(arrayString);
            }
            else if (input is IEnumerable<ActivityEvent> events)
            {
                return MapAsString(events);
            }
            else if (input is IEnumerable<ActivityLink> links)
            {
                return MapAsString(links);
            }
            else if (input is ActivityContext context)
            {
                return MapAsString(context);
            }
            else if (input is ReadOnlyTagCollection tags)
            {
                return MapAsString(tags);
            }
            else if (input is bool b)
            {
                return b ? "true" : "false";
            }
            else if (input is double d)
            {
                if (double.IsPositiveInfinity(d))
                {
                    return "'Infinity'";
                }
                else if (double .IsNegativeInfinity(d))
                {
                    return "'-Infinity'";
                }
                else if (double.IsNaN(d))
                {
                    return "'NaN'";
                }
            }
            else if (input is float f)
            {
                if (float.IsPositiveInfinity(f))
                {
                    return "'Infinity'";
                }
                else if (float.IsNegativeInfinity(f))
                {
                    return "'-Infinity'";
                }
                else if (float.IsNaN(f))
                {
                    return "'NaN'";
                }
            }
            else if (input is Enum e)
            {
                return Enum.Format(typeof(T), e, "D");
            }
            return input.ToString()!;
        }
        private static void MapAsString(ref ValueStringBuilder s,MetricType metricType,in MetricPoint point)
        {
            s.Append("{");
            if (metricType == MetricType.Histogram || metricType == MetricType.ExponentialHistogram)
            {
                s.Append("'value':NULL,'sum':");
                s.Append(WrapValue(point.GetHistogramSum()));
                s.Append(",'count':");
                s.Append(WrapValue(point.GetHistogramCount()));
                s.Append(',');
                if (point.TryGetHistogramMinMaxValues(out double min, out double max))
                {
                    s.Append("'min':");
                    s.Append(WrapValue(min));
                    s.Append(",'max':");
                    s.Append(WrapValue(max));
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
                            s.Append(WrapValue(double.NegativeInfinity)+"::DOUBLE");
                            s.Append(",'rangeRight':");
                            s.Append(WrapValue(histogramMeasurement.ExplicitBound));

                            s.Append(",'bucketCount':");
                            s.Append(WrapValue(histogramMeasurement.BucketCount));
                            previousExplicitBound = histogramMeasurement.ExplicitBound;
                            isFirstIteration = false;
                        }
                        else
                        {
                            s.Append("'rangeLeft':");
                            s.Append(WrapValue(previousExplicitBound));
                            s.Append(",'rangeRight':");

                            if (histogramMeasurement.ExplicitBound != double.PositiveInfinity)
                            {
                                s.Append(WrapValue(histogramMeasurement.ExplicitBound));
                                previousExplicitBound = histogramMeasurement.ExplicitBound;
                            }
                            else
                            {
                                s.Append(WrapValue(double.PositiveInfinity) + "::DOUBLE");
                            }

                            s.Append(",'bucketCount':");
                            s.Append(WrapValue(histogramMeasurement.BucketCount));
                        }
                        s.Append("}");
                    }
                    //s.Remove(s.Length - 1, 1);
                }
                else
                {
                    var exponentialHistogramData = point.GetExponentialHistogramData();
                    s.Append("'histogram':NULL,zeroCount':");
                    s.Append(WrapValue(exponentialHistogramData.ZeroCount));
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
                        s.Append(WrapValue(Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale)));
                        s.Append(",\"upperBound\":");
                        s.Append(WrapValue(Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale)));
                        s.Append(",\"bucketCount\":");
                        s.Append(WrapValue(bucketCount));
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
                        s.Append(WrapValue(point.GetSumDouble()));
                    }
                    else
                    {
                        s.Append(WrapValue(point.GetGaugeLastValueDouble()));
                    }
                }
                else if (metricType.IsLong())
                {
                    if (metricType.IsSum())
                    {
                        s.Append(WrapValue(point.GetSumLong()));
                    }
                    else
                    {
                        s.Append(WrapValue(point.GetGaugeLastValueLong()));
                    }
                }
                s.Append(",'sum':NULL,'min':NULL,'max':NULL,'count':NULL,'histogram':NULL,'zeroBucketCount':NULL,'buckets':NULL,");
            }
            s.Append("'startTime':");
            s.Append(WrapValue(point.StartTime));
            s.Append(",'endTime':");
            s.Append(WrapValue(point.StartTime));
            s.Append(",'tags':");
            s.Append(WrapValue(point.Tags));
            s.Append("}");
        }
        internal static void MapAsString(ref ValueStringBuilder s, MetricType metricType,in MetricPointsAccessor points)
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
        private static string MapAsString(in ActivityContext context)
        {
            using var s = new ValueStringBuilder();
            s.Append("{");
            s.Append("'traceId':");
            s.Append(WrapValue(context.TraceId.ToString()));
            s.Append(",'traceState':");
            s.Append(WrapValue(context.TraceState));
            s.Append(",'traceFlags':");
            s.Append(WrapValue(context.TraceFlags));
            s.Append(",'isRemote':");
            s.Append(WrapValue(context.IsRemote));
            s.Append(",'spanId':");
            s.Append(WrapValue(context.SpanId.ToString()));
            s.Append('}');
            return s.ToString();
        }

        private static string MapAsString(IEnumerable<ActivityLink> links)
        {
            if (!links.Any())
            {
                return "ARRAY []";
            }

            var isFirst = true;
            using var s = new ValueStringBuilder();
            s.Append("ARRAY [");
            foreach (var item in links)
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
                s.Append("'context':");
                s.Append(WrapValue(item.Context));
                s.Append(",'tags':");
                s.Append(WrapValue(item.Tags));
                s.Append("}");
            }

            //s.Remove(s.Length - 1, 1);
            s.Append(']');
            return s.ToString();

        }
        private static string MapAsString(IEnumerable<ActivityEvent> events)
        {
            if (!events.Any())
            {
                return "ARRAY []";
            }

            var isFirst = true;
            using var s = new ValueStringBuilder();
            s.Append("ARRAY [");
            foreach (var item in events)
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
                s.Append("'name':");
                s.Append(WrapValue(item.Name));
                s.Append(",'timestamp':");
                s.Append(WrapValue(item.Timestamp));
                s.Append(",'tags':");
                s.Append(WrapValue(item.Tags));
                s.Append("}");
            }
            //s.Remove(s.Length - 1, 1);
            s.Append(']');
            return s.ToString();
        }
        private static string MapAsString(in ReadOnlyTagCollection tags)
        {
            if (tags.Count==0)
            {
                return "MAP {}";
            }
            var isFirst = true;
            using var s = new ValueStringBuilder();
            s.Append("MAP {");
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
                s.Append(WrapValue(item.Key));
                s.Append(':');
                s.Append(WrapValue(item.Value?.ToString()));
            }
            //s.Remove(s.Length - 1, 1);
            s.Append('}');
            return s.ToString();
        }
        private static string MapAsString<T>(IEnumerable<KeyValuePair<string, T>> arrayObject)
        {
            if (!arrayObject.Any())
            {
                return "MAP {}";
            }
            var isFirst = true;
            using var s = new ValueStringBuilder();
            s.Append("MAP {");
            foreach (var item in arrayObject)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    s.Append(',');
                }
                s.Append(WrapValue(item.Key));
                s.Append(':');
                s.Append(WrapValue(item.Value?.ToString()));
            }
            //s.Remove(s.Length-1, 1);
            s.Append('}');
            return s.ToString();
        }
    }
}
