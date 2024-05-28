using OpenTelemetry.Metrics;
using System.Globalization;
using System.Text;

namespace Diagnostics.Traces.Zips
{
    public static class MetricExportHelper
    {
        public static void ExportMetricString(TextWriter msg, Metric metric)
        {
            var unit = string.IsNullOrEmpty(metric.Unit) ? string.Empty : $"({metric.Unit})";
            var metricName = string.IsNullOrEmpty(metric.MeterName) ? string.Empty : metric.MeterName;
            var metricVersion = string.IsNullOrEmpty(metric.MeterVersion) ? string.Empty : metric.MeterVersion;
            var metricInfo = string.Empty;
            if (!string.IsNullOrEmpty(metricName))
            {
                metricInfo = $"[{metricName}:{metricVersion}]";
            }
            var tags = "{}";
            if (metric.MeterTags != null && metric.MeterTags.Any())
            {
                tags = $"{{ {string.Join(", ", metric.MeterTags.Select(x => $"{x.Key}:{x.Value}"))} }}";
            }
            msg.Write("[[ ");
            msg.WriteLine($"{metric.Name}{metricInfo} {tags} {metric.MetricType}");

#if false
            [[cc [test:] {}
#endif
            var metricType = metric.MetricType;
            foreach (ref readonly var metricPoint in metric.GetMetricPoints())
            {
                msg.WriteLine();
                string valueDisplay = string.Empty;


                if (metricType == MetricType.Histogram || metricType == MetricType.ExponentialHistogram)
                {
                    var bucketsBuilder = new StringBuilder();
                    var sum = metricPoint.GetHistogramSum();
                    var count = metricPoint.GetHistogramCount();
                    bucketsBuilder.Append($"Sum: {sum} Count: {count} ");
                    if (metricPoint.TryGetHistogramMinMaxValues(out double min, out double max))
                    {
                        bucketsBuilder.Append($"Min: {min} Max: {max} ");
                    }

                    bucketsBuilder.AppendLine();

                    if (metricType == MetricType.Histogram)
                    {
                        bool isFirstIteration = true;
                        double previousExplicitBound = default;
                        foreach (var histogramMeasurement in metricPoint.GetHistogramBuckets())
                        {
                            if (isFirstIteration)
                            {
                                bucketsBuilder.Append("(-Infinity,");
                                bucketsBuilder.Append(histogramMeasurement.ExplicitBound);
                                bucketsBuilder.Append(']');
                                bucketsBuilder.Append(':');
                                bucketsBuilder.Append(histogramMeasurement.BucketCount);
                                previousExplicitBound = histogramMeasurement.ExplicitBound;
                                isFirstIteration = false;
                            }
                            else
                            {
                                bucketsBuilder.Append('(');
                                bucketsBuilder.Append(previousExplicitBound);
                                bucketsBuilder.Append(',');
                                if (histogramMeasurement.ExplicitBound != double.PositiveInfinity)
                                {
                                    bucketsBuilder.Append(histogramMeasurement.ExplicitBound);
                                    previousExplicitBound = histogramMeasurement.ExplicitBound;
                                }
                                else
                                {
                                    bucketsBuilder.Append("+Infinity");
                                }

                                bucketsBuilder.Append(']');
                                bucketsBuilder.Append(':');
                                bucketsBuilder.Append(histogramMeasurement.BucketCount);
                            }

                            bucketsBuilder.AppendLine();
                        }
                    }
                    else
                    {
                        var exponentialHistogramData = metricPoint.GetExponentialHistogramData();
                        var scale = exponentialHistogramData.Scale;

                        if (exponentialHistogramData.ZeroCount != 0)
                        {
                            bucketsBuilder.AppendLine($"Zero Bucket:{exponentialHistogramData.ZeroCount}");
                        }

                        var offset = exponentialHistogramData.PositiveBuckets.Offset;
                        foreach (var bucketCount in exponentialHistogramData.PositiveBuckets)
                        {
                            var lowerBound = Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale).ToString(CultureInfo.InvariantCulture);
                            var upperBound = Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(++offset, scale).ToString(CultureInfo.InvariantCulture);
                            bucketsBuilder.AppendLine($"({lowerBound}, {upperBound}]:{bucketCount}");
                        }
                    }

                    valueDisplay = bucketsBuilder.ToString();
                }
                else if (metricType.IsDouble())
                {
                    if (metricType.IsSum())
                    {
                        valueDisplay = metricPoint.GetSumDouble().ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        valueDisplay = metricPoint.GetGaugeLastValueDouble().ToString(CultureInfo.InvariantCulture);
                    }
                }
                else if (metricType.IsLong())
                {
                    if (metricType.IsSum())
                    {
                        valueDisplay = metricPoint.GetSumLong().ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        valueDisplay = metricPoint.GetGaugeLastValueLong().ToString(CultureInfo.InvariantCulture);
                    }
                }

                if (metricPoint.Tags.Count != 0)
                {
                    msg.Write(' ');
                    msg.Write('{');
                    var count = metricPoint.Tags.Count;
                    foreach (var tag in metricPoint.Tags)
                    {
                        msg.Write(tag.Key);
                        msg.Write(':');
                        msg.Write(tag.Value);
                        if (--count != 0)
                        {
                            msg.Write(',');
                        }
                    }
                    msg.Write("} " + metricPoint.Tags.Count);
                }
                msg.Write(' ');
                msg.Write('(');
                msg.Write(metricPoint.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture));
                msg.Write(", ");
                msg.Write(metricPoint.EndTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture));
                msg.Write("] ");
                msg.WriteLine();

                msg.Write(valueDisplay);
                msg.Write(' ');

                msg.WriteLine();
            }

            msg.WriteLine("]]");
        }
    }
}
