using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diagnostics.Traces.Parquet
{
    internal class MetricPointsAccessorJsonConverter : JsonConverter<Metric>
    {
        public static readonly MetricPointsAccessorJsonConverter Instance = new MetricPointsAccessorJsonConverter();

        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { Instance },
        };

        public override Metric Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Metric value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (ref readonly var item in value.GetMetricPoints())
            {
                writer.WriteStartObject();

                if (value.MetricType== MetricType.Histogram||value.MetricType== MetricType.ExponentialHistogram)
                {
                    writer.WriteNull("value");
                    writer.WriteNumber("sum", item.GetHistogramSum());
                    writer.WriteNumber("count", item.GetHistogramCount());
                    if (item.TryGetHistogramMinMaxValues(out double min,out double max))
                    {
                        writer.WriteNumber("min", min);
                        writer.WriteNumber("max", max);
                    }
                    else
                    {
                        writer.WriteNull("min");
                        writer.WriteNull("max");
                    }

                    writer.WriteStartArray("histogram");
                    if (value.MetricType == MetricType.Histogram)
                    {

                        var isFirstIteration = true;
                        var previousExplicitBound = 0d;
                        foreach (var histogramMeasurement in item.GetHistogramBuckets())
                        {
                            writer.WriteStartObject();
                            if (isFirstIteration)
                            {
                                writer.WriteNumber("rangeLeft", double.NegativeInfinity);
                                writer.WriteNumber("rangeRight", histogramMeasurement.ExplicitBound);
                                writer.WriteNumber("bucketCount", histogramMeasurement.BucketCount);
                                previousExplicitBound = histogramMeasurement.ExplicitBound;
                                isFirstIteration = false;
                            }
                            else
                            {
                                writer.WriteNumber("rangeLeft", previousExplicitBound);
                                if (histogramMeasurement.ExplicitBound != double.PositiveInfinity)
                                {
                                    writer.WriteNumber("rangeRight", histogramMeasurement.ExplicitBound);
                                    previousExplicitBound = histogramMeasurement.ExplicitBound;
                                }
                                else
                                {
                                    writer.WriteNumber("rangeRight", double.PositiveInfinity);
                                }
                                writer.WriteNumber("bucketCount", histogramMeasurement.BucketCount);
                            }
                            writer.WriteEndObject();
                        }
                    }
                    else
                    {
                        var exponentialHistogramData = item.GetExponentialHistogramData();
                        writer.WriteNull("histogram");
                        writer.WriteNumber("zeroCount", exponentialHistogramData.ZeroCount);
                        writer.WriteStartArray();
                        var scale = exponentialHistogramData.Scale;
                        var offset = exponentialHistogramData.PositiveBuckets.Offset;
                        foreach (var bucketCount in exponentialHistogramData.PositiveBuckets)
                        {
                            writer.WriteNumber("lowerBound", Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale));
                            writer.WriteNumber("upperBound", Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale));
                            writer.WriteNumber("bucketCount", bucketCount);
                        }

                        writer.WriteEndArray();
                    }
                    writer.WriteEndArray();
                }
                else
                {
                    if (value.MetricType.IsDouble())
                    {
                        if (value.MetricType.IsSum())
                        {
                            writer.WriteNumber("value", item.GetSumDouble());
                        }
                        else
                        {
                            writer.WriteNumber("value", item.GetGaugeLastValueDouble());
                        }
                    }
                    else if (value.MetricType.IsLong())
                    {
                        if (value.MetricType.IsSum())
                        {
                            writer.WriteNumber("value", item.GetSumLong());
                        }
                        else
                        {
                            writer.WriteNumber("value", item.GetGaugeLastValueLong());
                        }
                    }
                }

                writer.WriteString("startTime", item.StartTime);
                writer.WriteString("endTime", item.EndTime);
                writer.WriteStartObject("tags");

                if (item.Tags.Count!=0)
                {
                    foreach (var tag in item.Tags)
                    {
                        writer.WriteString(tag.Key, tag.Value?.ToString());
                    }
                }

                writer.WriteEndObject();

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
    internal class AttributeJsonConverter : JsonConverter<IReadOnlyList<KeyValuePair<string, object?>>>
    {
        public static readonly AttributeJsonConverter Instance = new AttributeJsonConverter();

        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { Instance },
        };

        public override IReadOnlyList<KeyValuePair<string, object?>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IReadOnlyList<KeyValuePair<string, object?>> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (value != null)
            {
                foreach (var item in value)
                {
                    writer.WriteString(item.Key, item.Value?.ToString());
                }
            }
            writer.WriteEndObject();
        }
    }
    internal class TagsJsonConverter : JsonConverter<IReadOnlyList<KeyValuePair<string, string?>>>
    {
        public static readonly TagsJsonConverter Instance = new TagsJsonConverter();

        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { Instance },
        };

        public override IReadOnlyList<KeyValuePair<string, string?>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IReadOnlyList<KeyValuePair<string, string?>> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (value != null)
            {
                foreach (var item in value)
                {
                    writer.WriteString(item.Key, item.Value);
                }
            }
            writer.WriteEndObject();
        }
    }


    internal class ActivityEventJsonConverter : JsonConverter<ActivityEvent>
    {

        public static readonly ActivityEventJsonConverter Instance = new ActivityEventJsonConverter();

        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { Instance },
        };
        public override ActivityEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public static void WriteEvent(Utf8JsonWriter writer, ActivityEvent value)
        {

            writer.WriteStartObject();
            writer.WriteString("name", value.Name);
            writer.WriteString("timestamp", value.Timestamp);
            writer.WriteStartObject("tags");
            if (value.Tags != null)
            {
                foreach (var item in value.Tags)
                {
                    writer.WriteString(item.Key, item.Value?.ToString());
                }
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        public override void Write(Utf8JsonWriter writer, ActivityEvent value, JsonSerializerOptions options)
        {
            WriteEvent(writer, value);
        }
    }

    internal class ActivityContextJsonConverter : JsonConverter<ActivityContext>
    {
        public static readonly ActivityContextJsonConverter Instance = new ActivityContextJsonConverter();

        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { Instance },
        };
        public override ActivityContext Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ActivityContext value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("traceId", value.TraceId.ToString());
            writer.WriteString("traceState", value.TraceState);
            writer.WriteNumber("traceFlags", (int)value.TraceFlags);
            writer.WriteBoolean("isRemote", value.IsRemote);
            writer.WriteString("spanId", value.SpanId.ToString());
            writer.WriteEndObject();
        }
    }

    internal class ActivityLinksJsonConverter : JsonConverter<ActivityLink>
    {
        public static readonly ActivityLinksJsonConverter Instance = new ActivityLinksJsonConverter();

        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = { Instance },
        };
        public override ActivityLink Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ActivityLink value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteStartObject("context");
            writer.WriteString("traceId", value.Context.TraceId.ToString());
            writer.WriteString("traceState", value.Context.TraceState);
            writer.WriteNumber("traceFlags", (int)value.Context.TraceFlags);
            writer.WriteBoolean("isRemote", value.Context.IsRemote);
            writer.WriteString("spanId", value.Context.SpanId.ToString());
            writer.WriteEndObject();

            writer.WriteStartObject("tags");
            if (value.Tags!=null)
            {
                foreach (var item in value.Tags)
                {
                    writer.WriteString(item.Key, item.Value?.ToString());
                }
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }

}
