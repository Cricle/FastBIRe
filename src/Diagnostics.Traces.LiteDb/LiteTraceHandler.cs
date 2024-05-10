using CommunityToolkit.HighPerformance.Buffers;
using LiteDB;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Diagnostics.Traces.LiteDb
{
    public class LiteTraceHandler<TIdentity> : IActivityTraceHandler, ILogRecordTraceHandler, IMetricTraceHandler, IBatchActivityTraceHandler, IBatchLogRecordTraceHandler, IBatchMetricTraceHandler, IDisposable
        where TIdentity : IEquatable<TIdentity>
    {
        public LiteTraceHandler(ILiteDatabaseSelector<TIdentity> databaseSelector,
            IIdentityProvider<TIdentity, Activity>? activityIdentityProvider,
            IIdentityProvider<TIdentity, LogRecord>? logIdentityProvider,
            IIdentityProvider<TIdentity, Metric>? metricIdentityProvider=null)
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

        private bool TryCreateActivityDocument(Activity activity,out TIdentity? identity, out BsonDocument? doc)
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
            using (var bufferWriter = new ArrayPoolBufferWriter<byte>())
            {
                using (var utf8Writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions
                {
                    SkipValidation =
#if DEBUG 
                       false
#else
                       true
#endif
                }))
                {
                    ActivityJsonConverter.Write(utf8Writer, activity);
                }
                var str = Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
                doc = LiteDB.JsonSerializer.Deserialize(str).AsDocument;

                return true;
            }
        }

        public void Handle(Activity input)
        {
            if (TryCreateActivityDocument(input,out var identity,out var doc)&&identity!=null)
            {
                var database = DatabaseSelector.GetLiteDatabase(TraceTypes.Activity);
                var coll = database.GetCollection("activities");
                coll.Insert(doc);
            }
        }

        private bool TryCreateLogDocument(LogRecord input,out TIdentity? identity,out BsonDocument? doc)
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
            identity=res.Identity;
            doc = new BsonDocument(new Dictionary<string, BsonValue>()
            {
                ["timestamp"] = input.Timestamp,
                ["logLevel"] = input.LogLevel.ToString(),
                ["categoryName"] = input.CategoryName,
                ["traceId"] = input.TraceId.ToString(),
                ["spanId"] = input.SpanId.ToString(),
                ["attributes"] = LiteDB.JsonSerializer.Deserialize(System.Text.Json.JsonSerializer.Serialize(input.Attributes)),
                ["formattedMessage"] = input.FormattedMessage,
                ["body"] = input.Body
            });
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
        private int count = 0;
        public void Handle(in Batch<LogRecord> inputs)
        {
            var docs = new List<BsonDocument>((int)inputs.Count);
            foreach (var item in inputs)
            {
                if (TryCreateLogDocument(item,out _,out var doc)&&doc!=null)
                {
                    docs.Add(doc);
                }
            }
            if (docs.Count!=0)
            {
                var database = DatabaseSelector.GetLiteDatabase(TraceTypes.Activity);
                var coll = database.GetCollection("logs");
                coll.InsertBulk(docs);
                count += docs.Count;
                Console.WriteLine(count);
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
            var docs = new List<BsonDocument>((int)inputs.Count);
            foreach (var item in inputs)
            {
                if (TryCreateActivityDocument(item, out _, out var doc) && doc != null)
                {
                    docs.Add(doc);
                }
            }
            if (docs.Count != 0)
            {
                var database = DatabaseSelector.GetLiteDatabase(TraceTypes.Activity);
                var coll = database.GetCollection("activities", BsonAutoId.Int64);
                coll.InsertBulk(docs);
            }
        }
    }
}
