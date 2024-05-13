using OpenTelemetry.Metrics;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

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
            else if (input is bool b)
            {
                return b ? "true" : "false";
            }
            else if (input is double d)
            {
                if (double.IsPositiveInfinity(d))
                {
                    return "'+Infinity'";
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
                    return "'+Infinity'";
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
        private static void MapAsString(StringBuilder s,MetricType metricType,in MetricPoint point)
        {
            s.Append("JSON_ARRAY(");
            if (metricType == MetricType.Histogram || metricType == MetricType.ExponentialHistogram)
            {
                s.Append(WrapValue(point.GetSumDouble()));
                s.Append(',');
                s.Append(WrapValue(point.GetHistogramCount()));
                s.Append(',');
                if (point.TryGetHistogramMinMaxValues(out double min, out double max))
                {
                    s.Append(WrapValue(min));
                    s.Append(',');
                    s.Append(WrapValue(max));
                    s.Append(',');
                }
                if (metricType == MetricType.Histogram)
                {
                    var isFirstIteration = true;
                    var previousExplicitBound = 0d;

                    s.Append("JSON_ARRAY(");
                    foreach (var histogramMeasurement in point.GetHistogramBuckets())
                    {
                        s.Append("JSON_OBJECT(");
                        if (isFirstIteration)
                        {
                            s.Append("\"rangeLeft\",");
                            s.Append(WrapValue(double.NegativeInfinity));
                            s.Append(",\"rangeRight\",");
                            s.Append(WrapValue(histogramMeasurement.ExplicitBound));

                            s.Append(",\"bucketCount\",");
                            s.Append(WrapValue(histogramMeasurement.BucketCount));
                        }
                        else
                        {
                            s.Append("\"rangeLeft\",");
                            s.Append(WrapValue(previousExplicitBound));
                            s.Append(",\"rangeRight\",");

                            if (histogramMeasurement.ExplicitBound != double.PositiveInfinity)
                            {
                                s.Append(WrapValue(histogramMeasurement.ExplicitBound));
                            }
                            else
                            {
                                s.Append(WrapValue(double.PositiveInfinity));
                            }

                            s.Append(",\"bucketCount\",");
                            s.Append(WrapValue(histogramMeasurement.BucketCount));
                        }
                        s.Append("),");
                    }
                    s.Append("NULL,JSON_ARRAY())");
                }
                else
                {
                    var exponentialHistogramData = point.GetExponentialHistogramData();
                    s.Append("JSON_ARRAY(),");
                    s.Append(WrapValue(exponentialHistogramData.ZeroCount));
                    s.Append("JSON_ARRAY(");

                    var scale = exponentialHistogramData.Scale;
                    var offset = exponentialHistogramData.PositiveBuckets.Offset;

                    foreach (var bucketCount in exponentialHistogramData.PositiveBuckets)
                    {
                        s.Append("JSON_OBJECT(");
                        s.Append("\"lowerBound\",");
                        s.Append(WrapValue(Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale)));
                        s.Append(",\"upperBound\",");
                        s.Append(WrapValue(Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale)));
                        s.Append(",\"bucketCount\",");
                        s.Append(WrapValue(bucketCount));
                        s.Append("),");
                    }
                    s.Remove(s.Length - 1, 1);
                    s.Append(')');
                }
            }
            s.Append(')');
        }
        internal static void MapAsString(StringBuilder s, MetricType metricType,in MetricPointsAccessor points)
        {
            s.Append("JSON_ARRAY(");
            foreach (ref readonly var item in points)
            {
                MapAsString(s, metricType, item);
                s.Append(',');
            }
            s.Remove(s.Length - 1, 1);
            s.Append(')');
        }
        private static string MapAsString(in ActivityContext context)
        {
            var s = new StringBuilder("JSON_OBJECT(");
            s.Append("'traceId',");
            s.Append(WrapValue(context.TraceId.ToString()));
            s.Append(",'traceState',");
            s.Append(WrapValue(context.TraceState));
            s.Append(",'traceFlags',");
            s.Append(WrapValue(context.TraceFlags));
            s.Append(",'isRemote',");
            s.Append(WrapValue(context.IsRemote));
            s.Append(",'spanId',");
            s.Append(WrapValue(context.SpanId.ToString()));
            s.Append(')');
            return s.ToString();
        }

        private static string MapAsString(IEnumerable<ActivityLink> links)
        {
            if (!links.Any())
            {
                return "JSON_ARRAY()";
            }


            var s = new StringBuilder("JSON_ARRAY(");
            foreach (var item in links)
            {
                s.Append("JSON_OBJECT(");
                s.Append("'context',");
                s.Append(WrapValue(item.Context));
                s.Append(",'tags',");
                s.Append(WrapValue(item.Tags));
                s.Append("),");
            }
            s.Remove(s.Length - 1, 1);
            s.Append(')');
            return s.ToString();

        }
        private static string MapAsString(IEnumerable<ActivityEvent> events)
        {
            if (!events.Any())
            {
                return "JSON_ARRAY()";
            }


            var s = new StringBuilder("JSON_ARRAY(");
            foreach (var item in events)
            {
                s.Append("JSON_OBJECT(");
                s.Append("'name',");
                s.Append(WrapValue(item.Name));
                s.Append(",'timestamp',");
                s.Append(WrapValue(item.Timestamp));
                s.Append(",'tags',");
                s.Append(WrapValue(item.Tags));
                s.Append("),");
            }
            s.Remove(s.Length - 1, 1);
            s.Append(')');
            return s.ToString();
        }
        private static string MapAsString<T>(IEnumerable<KeyValuePair<string, T>> arrayObject)
        {
            if (!arrayObject.Any())
            {
                return "JSON_OBJECT()";
            }
            var s = new StringBuilder("JSON_OBJECT(");
            foreach (var item in arrayObject)
            {
                s.Append(WrapValue(item.Key));
                s.Append(',');
                s.Append(WrapValue(item.Value?.ToString()));
                s.Append(',');
            }
            s.Remove(s.Length-1, 1);
            s.Append(')');
            return s.ToString();
        }
    }
}
