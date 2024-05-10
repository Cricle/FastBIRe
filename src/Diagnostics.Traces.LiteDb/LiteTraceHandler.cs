using LiteDB;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Buffers;
using System.Diagnostics;

namespace Diagnostics.Traces.LiteDb
{

    public class LiteTraceHandler<TIdentity> : IActivityTraceHandler, ILogRecordTraceHandler, IMetricTraceHandler, IBatchActivityTraceHandler, IBatchLogRecordTraceHandler, IBatchMetricTraceHandler, IDisposable
        where TIdentity : IEquatable<TIdentity>
    {
        public LiteTraceHandler(ILiteDatabaseSelector<TIdentity> databaseSelector,
            IIdentityProvider<TIdentity, Activity>? activityIdentityProvider,
            IIdentityProvider<TIdentity, LogRecord>? logIdentityProvider,
            IIdentityProvider<TIdentity, Metric>? metricIdentityProvider = null)
        {
            DatabaseSelector = databaseSelector;
            ActivityIdentityProvider = activityIdentityProvider;
            LogIdentityProvider = logIdentityProvider;
            MetricIdentityProvider = metricIdentityProvider;

        }


        public ILiteDatabaseSelector<TIdentity> DatabaseSelector { get; }

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

        public void Handle(Activity input)
        {
            if (TryCreateActivityDocument(input, out var identity, out var doc) && identity != null)
            {
                var database = DatabaseSelector.GetLiteDatabase(TraceTypes.Activity);
                var coll = database.GetCollection("activities");
                coll.Insert(doc);
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
            doc["logLevel"] = input.LogLevel.ToString();
            doc["categoryName"] = input.CategoryName;
            doc["traceId"] = input.TraceId.ToString();
            doc["spanId"] = input.SpanId.ToString();
            var arr = new BsonArray();
            if (input.Attributes != null && input.Attributes.Count != 0)
            {
                foreach (var item in input.Attributes)
                {
                    arr.Add(new BsonDocument
                    {
                        [item.Key] = item.Value?.ToString()
                    });
                }
            }
            doc["attributes"] = arr;
            doc["formattedMessage"] = input.FormattedMessage;
            doc["body"] = input.Body;
            return true;
        }

        public void Handle(LogRecord input)
        {
            if (TryCreateLogDocument(input, out var identity, out var doc) && identity != null)
            {
                var database = DatabaseSelector.GetLiteDatabase(TraceTypes.Log);
                var coll = database.GetCollection("logs");
                coll.Insert(doc);
            }
        }

        public void Handle(Metric input)
        {
        }

        public Task HandleAsync(Activity input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(input);
            return Task.CompletedTask;
        }

        public Task HandleAsync(LogRecord input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(input);
            return Task.CompletedTask;
        }

        public Task HandleAsync(Metric input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(input);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }

        public Task HandleAsync(Batch<Metric> inputs, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(inputs);
            return Task.CompletedTask;
        }

        public void Handle(in Batch<Metric> inputs)
        {
        }

        public Task HandleAsync(Batch<LogRecord> inputs, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(inputs);
            return Task.CompletedTask;
        }

        public void Handle(in Batch<LogRecord> inputs)
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
                    var database = DatabaseSelector.GetLiteDatabase(TraceTypes.Log);
                    var coll = database.GetCollection("logs");
                    coll.InsertBulk(buffer.Take(index));
                }
            }
            finally
            {
                ArrayPool<BsonDocument>.Shared.Return(buffer);
            }
        }

        public Task HandleAsync(Batch<Activity> inputs, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(inputs);
            return Task.CompletedTask;
        }
        public void Handle(in Batch<Activity> inputs)
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
                    var database = DatabaseSelector.GetLiteDatabase(TraceTypes.Activity);
                    var coll = database.GetCollection("activities");
                    coll.InsertBulk(buffer.Take(index));
                }
            }
            finally
            {
                ArrayPool<BsonDocument>.Shared.Return(buffer);

            }
        }
    }
}
