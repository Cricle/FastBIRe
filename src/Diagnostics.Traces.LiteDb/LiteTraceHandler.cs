using Diagnostics.Traces.Stores;
using LiteDB;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Buffers;
using System.Diagnostics;

namespace Diagnostics.Traces.LiteDb
{
    public class LiteTraceHandler<TIdentity> : TraceHandlerBase<TIdentity>
        where TIdentity : IEquatable<TIdentity>
    {
        public LiteTraceHandler(IUndefinedDatabaseSelector<LiteDatabaseCreatedResult> databaseSelector,
            IIdentityProvider<TIdentity, Activity>? activityIdentityProvider,
            IIdentityProvider<TIdentity, LogRecord>? logIdentityProvider,
            IIdentityProvider<TIdentity, Metric>? metricIdentityProvider = null)
        {
            DatabaseSelector = databaseSelector;
            ActivityIdentityProvider = activityIdentityProvider;
            LogIdentityProvider = logIdentityProvider;
            MetricIdentityProvider = metricIdentityProvider;
        }

        public IUndefinedDatabaseSelector<LiteDatabaseCreatedResult> DatabaseSelector { get; }

        public IIdentityProvider<TIdentity, Activity>? ActivityIdentityProvider { get; }

        public IIdentityProvider<TIdentity, LogRecord>? LogIdentityProvider { get; }

        public IIdentityProvider<TIdentity, Metric>? MetricIdentityProvider { get; }

        private bool TryCreateActivityDocument(Activity activity, out TIdentity? identity, out BsonDocument? doc)
        {
            doc = null;
            identity = default;

            if (ActivityIdentityProvider == null)
            {
                return false;
            }
            var res = ActivityIdentityProvider.GetIdentity(activity);
            if (!res.Succeed || res.Identity == null)
            {
                return false;
            }
            identity = res.Identity;
            doc = new BsonDocument();
            ActivityToLiteHelper.Write(doc, activity);

            return true;
        }

        public override void Handle(Activity input)
        {
            if (TryCreateActivityDocument(input, out var identity, out var doc) && identity != null)
            {
                DatabaseSelector.UsingDatabaseResult(TraceTypes.Activity, res =>
                {
                    var coll = res.Database.GetCollection(LiteTraceCollectionNames.Activity);
                    coll.Insert(doc);
                    DatabaseSelector.ReportInserted(TraceTypes.Activity, 1);
                });
            }
        }

        private bool TryCreateLogDocument(LogRecord input, out TIdentity? identity, out BsonDocument? doc)
        {
            identity = default;
            doc = null;

            if (LogIdentityProvider == null)
            {
                return false;
            }
            var res = LogIdentityProvider.GetIdentity(input);
            if (!res.Succeed || res.Identity == null)
            {
                return false;
            }
            identity = res.Identity;
            doc = new BsonDocument();
            doc["timestamp"] = input.Timestamp;
            doc["logLevel"] = string.Intern(input.LogLevel.ToString());
            doc["categoryName"] = input.CategoryName;
            doc["traceId"] = input.TraceId.ToString();
            doc["spanId"] = input.SpanId.ToString();
            var arr = new BsonDocument();
            if (input.Attributes != null && input.Attributes.Count != 0)
            {
                foreach (var item in input.Attributes)
                {
                    arr[item.Key] = item.Value?.ToString();
                }
            }
            doc["attributes"] = arr;
            doc["formattedMessage"] = input.FormattedMessage;
            doc["body"] = input.Body;
            return true;
        }

        public override void Handle(LogRecord input)
        {
            if (TryCreateLogDocument(input, out var identity, out var doc) && identity != null)
            {
                DatabaseSelector.UsingDatabaseResult(TraceTypes.Log, res =>
                {
                    var coll = res.Database.GetCollection(LiteTraceCollectionNames.Log);
                    coll.Insert(doc);
                    DatabaseSelector.ReportInserted(TraceTypes.Log, 1);
                });
            }
        }

        public override void Handle(Metric input)
        {
            if (TryCreateMetricDocument(input, out var identity, out var doc) && identity != null)
            {
                DatabaseSelector.UsingDatabaseResult(TraceTypes.Metric, res =>
                {
                    var coll = res.Database.GetCollection(LiteTraceCollectionNames.Metrics);
                    coll.Insert(doc);
                    DatabaseSelector.ReportInserted(TraceTypes.Metric, 1);
                });
            }
        }

        private bool TryCreateMetricDocument(Metric input, out TIdentity? identity, out BsonDocument? doc)
        {
            identity = default;
            doc = null;

            if (MetricIdentityProvider == null)
            {
                return false;
            }
            var res = MetricIdentityProvider.GetIdentity(input);
            if (!res.Succeed || res.Identity == null)
            {
                return false;
            }
            identity = res.Identity;
            doc = new BsonDocument();
            doc["name"] = input.Name;
            doc["unit"] = input.Unit;
            doc["metricType"] = string.Intern(input.MetricType.ToString());
            doc["temporality"] = string.Intern(input.Temporality.ToString());
            doc["description"] = input.Description;
            doc["meterName"] = input.MeterName;
            doc["meterVersion"] = input.MeterVersion;
            var tags = new BsonDocument();
            if (input.MeterTags!=null)
            {
                foreach (var item in input.MeterTags)
                {
                    tags[item.Key] = item.Value?.ToString();
                }
            }
            doc["meterTags"] = tags;
            var points = new BsonArray();
            doc["points"] = points;
            var metricType = input.MetricType;

            foreach (ref readonly var metricPoint in input.GetMetricPoints())
            {
                var point = new BsonDocument();
                if (metricType == MetricType.Histogram|| metricType == MetricType.ExponentialHistogram)
                {
                    point["sum"] = metricPoint.GetSumDouble();
                    point["count"] = metricPoint.GetHistogramCount();
                    if (metricPoint.TryGetHistogramMinMaxValues(out double min, out double max))
                    {
                        point["min"] = min;
                        point["max"] = max;
                    }
                    if (metricType == MetricType.Histogram)
                    {
                        var histogramArray = new BsonArray();
                        var isFirstIteration = true;
                        var previousExplicitBound = 0d;
                        point["histogram"] = histogramArray;

                        foreach (var histogramMeasurement in metricPoint.GetHistogramBuckets())
                        {
                            var histogramDoc = new BsonDocument();
                            if (isFirstIteration)
                            {
                                histogramDoc["rangeLeft"] = "-Inf";
                                histogramDoc["rangeRight"] = histogramMeasurement.ExplicitBound;
                                histogramDoc["bucketCount"] = histogramMeasurement.BucketCount;
                                previousExplicitBound = histogramMeasurement.ExplicitBound;
                                isFirstIteration = false;
                            }
                            else
                            {
                                histogramDoc["rangeLeft"] = previousExplicitBound;
                                if (histogramMeasurement.ExplicitBound != double.PositiveInfinity)
                                {
                                    histogramDoc["rangeRight"]=histogramMeasurement.ExplicitBound;
                                    previousExplicitBound = histogramMeasurement.ExplicitBound;
                                }
                                else
                                {
                                    histogramDoc["rangeRight"] = "+Inf";
                                }
                                histogramDoc["bucketCount"] = histogramMeasurement.BucketCount;
                            }
                            histogramArray.Add(histogramDoc);
                        }
                    }
                    else
                    {
                        var exponentialHistogramDoc = new BsonDocument();
                        point["histogram"] = exponentialHistogramDoc;
                        var exponentialHistogramBuckets=new BsonArray();
                        exponentialHistogramDoc["buckets"] = exponentialHistogramBuckets;
                        var exponentialHistogramData = metricPoint.GetExponentialHistogramData();
                        var scale = exponentialHistogramData.Scale;

                        exponentialHistogramDoc["zeroBucketCount"] = exponentialHistogramData.ZeroCount;

                        var offset = exponentialHistogramData.PositiveBuckets.Offset;
                        foreach (var bucketCount in exponentialHistogramData.PositiveBuckets)
                        {
                            var lowerBound = Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(offset, scale);
                            var upperBound = Base2ExponentialBucketHistogramHelper.CalculateLowerBoundary(++offset, scale);
                            exponentialHistogramBuckets.Add(new BsonDocument
                            {
                                ["lowerBound"] = lowerBound,
                                ["upperBound"] = upperBound,
                                ["bucketCount"] = bucketCount
                            });
                        }
                    }
                }
                else if (metricType.IsDouble())
                {
                    if (metricType.IsSum())
                    {
                        point["value"] = metricPoint.GetSumDouble();
                    }
                    else
                    {
                        point["value"] = metricPoint.GetGaugeLastValueDouble();
                    }
                }
                else if (metricType.IsLong())
                {
                    if (metricType.IsSum())
                    {
                        point["value"] = metricPoint.GetSumLong();
                    }
                    else
                    {
                        point["value"] = metricPoint.GetGaugeLastValueLong();
                    }
                }
                var tagsDoc = new BsonDocument();
                if (metricPoint.Tags.Count!=0)
                {
                    foreach (var item in metricPoint.Tags)
                    {
                        tagsDoc[item.Key] = item.Value?.ToString();
                    }
                }
                point["startTime"] = metricPoint.StartTime.DateTime;
                point["endTime"] = metricPoint.EndTime.DateTime;

                point["tags"] = tagsDoc;

                points.Add(point);
            }
            return true;
        }
        public override void Handle(in Batch<Metric> inputs)
        {
            var buffer = ArrayPool<BsonDocument>.Shared.Rent((int)inputs.Count);
            try
            {
                var index = 0;
                foreach (var item in inputs)
                {
                    if (TryCreateMetricDocument(item, out _, out var doc) && doc != null)
                    {
                        buffer[index++] = doc;
                    }
                }
                if (index != 0)
                {
                    DatabaseSelector.UsingDatabaseResult(TraceTypes.Metric, res =>
                    {
                        var coll = res.Database.GetCollection(LiteTraceCollectionNames.Metrics);
                        coll.InsertBulk(buffer.Take(index));
                        DatabaseSelector.ReportInserted(TraceTypes.Metric, index);
                    });
                }
            }
            finally
            {
                ArrayPool<BsonDocument>.Shared.Return(buffer);
            }
        }
         
        public override void Handle(in Batch<LogRecord> inputs)
        {
            var buffer = ArrayPool<BsonDocument>.Shared.Rent((int)inputs.Count);
            try
            {
                var index = 0;
                foreach (var item in inputs)
                {
                    if (TryCreateLogDocument(item, out _, out var doc) && doc != null)
                    {
                        buffer[index++] = doc;
                    }
                }
                if (index != 0)
                {
                    DatabaseSelector.UsingDatabaseResult(TraceTypes.Log, res =>
                    {
                        var coll = res.Database.GetCollection(LiteTraceCollectionNames.Log);
                        coll.InsertBulk(buffer.Take(index));
                        DatabaseSelector.ReportInserted(TraceTypes.Log, index);
                    });
                }
            }
            finally
            {
                ArrayPool<BsonDocument>.Shared.Return(buffer);
            }
        }

        public override void Handle(in Batch<Activity> inputs)
        {
            var buffer = ArrayPool<BsonDocument>.Shared.Rent((int)inputs.Count);
            try
            {
                var index = 0;
                foreach (var item in inputs)
                {
                    if (TryCreateActivityDocument(item, out _, out var doc) && doc != null)
                    {
                        buffer[index++] = doc;
                    }
                }
                if (index != 0)
                {
                    DatabaseSelector.UsingDatabaseResult(TraceTypes.Activity, res =>
                    {
                        var coll = res.Database.GetCollection(LiteTraceCollectionNames.Activity);
                        coll.InsertBulk(buffer.Take(index));
                        DatabaseSelector.ReportInserted(TraceTypes.Activity, index);                    
                    });
                }
            }
            finally
            {
                ArrayPool<BsonDocument>.Shared.Return(buffer);

            }
        }
    }
}
