using Diagnostics.Traces.Stores;
using FastBIRe;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using ValueBuffer;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBTraceHandler<TIdentity> : TraceHandlerBase<TIdentity>
        where TIdentity : IEquatable<TIdentity>
    {
        public DuckDBTraceHandler(IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> databaseSelector,
            IIdentityProvider<TIdentity, Activity>? activityIdentityProvider,
            IIdentityProvider<TIdentity, LogRecord>? logIdentityProvider,
            IIdentityProvider<TIdentity, Metric>? metricIdentityProvider = null)
        {
            DatabaseSelector = databaseSelector;
            ActivityIdentityProvider = activityIdentityProvider;
            LogIdentityProvider = logIdentityProvider;
            MetricIdentityProvider = metricIdentityProvider;
        }

        public IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> DatabaseSelector { get; }

        public IIdentityProvider<TIdentity, Activity>? ActivityIdentityProvider { get; }

        public IIdentityProvider<TIdentity, LogRecord>? LogIdentityProvider { get; }

        public IIdentityProvider<TIdentity, Metric>? MetricIdentityProvider { get; }

        private string BuildSql(IEnumerator<LogRecord> logs)
        {
            using var s = new ValueStringBuilder();
            s.Append("INSERT INTO \"logs\" VALUES ");
            while (logs.MoveNext())
            {
                var item = logs.Current;

                s.Append('(');
                s.Append(DuckHelper.WrapValue(item.Timestamp));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.LogLevel));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.CategoryName));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.TraceId.ToString()));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.SpanId.ToString()));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Attributes));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.FormattedMessage));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Body));
                s.Append("),");
            }
            s._chars.RemoveLast(1);
            //s.Remove(s.Length - 1, 1);
            return s.ToString();
        }
        private string BuildSql(IEnumerator<Activity> activities)
        {
            using var s = new ValueStringBuilder();
            s.Append("INSERT INTO \"activities\" VALUES ");
            while (activities.MoveNext())
            {
                var item = activities.Current;

                s.Append('(');
                s.Append(DuckHelper.WrapValue(item.Id));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Status));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.StatusDescription));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.HasRemoteParent));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Kind));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.OperationName));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.DisplayName));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Source.Name));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Source.Version));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Duration.TotalMilliseconds));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.StartTimeUtc));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.ParentId));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.RootId));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Tags));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Events));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Links));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Baggage));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Context));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.TraceStateString));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.SpanId.ToString()));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.TraceId.ToString()));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Recorded));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.ActivityTraceFlags));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.ParentSpanId.ToString()));
                s.Append("),");
            }
            s._chars.RemoveLast(1);
            //s.Remove(s.Length - 1, 1);
            return s.ToString();
        }
        private string BuildSql(IEnumerator<Metric> metrics)
        {
            using var s = new ValueStringBuilder();
            s.Append("INSERT INTO \"metrics\" VALUES ");
            while (metrics.MoveNext())
            {
                var item = metrics.Current;

                s.Append('(');
                s.Append(DuckHelper.WrapValue(item.Name));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Unit));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.MetricType));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Temporality));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Description));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.MeterName));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.MeterVersion));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.MeterTags));
                s.Append(',');
                DuckHelper.MapAsString(in s, item.MetricType, item.GetMetricPoints());

                s.Append("),");
            }
            s._chars.RemoveLast(1);
            //s.Remove(s.Length - 1, 1);
            return s.ToString();
        }

        public override void Handle(Activity input)
        {
            var sql = BuildSql(new OneEnumerable<Activity>(input));
            DatabaseSelector.UsingDatabaseResult(TraceTypes.Activity, sql, static (res, sql) =>
            {
                res.Database.Execute(sql);
            });
            DatabaseSelector.ReportInserted(TraceTypes.Activity, 1);
        }

        public override void Handle(LogRecord input)
        {
            var sql = BuildSql(new OneEnumerable<LogRecord>(input));
            DatabaseSelector.UsingDatabaseResult(TraceTypes.Log, sql, static (res, sql) =>
            {
                res.Database.Execute(sql);
            });
            DatabaseSelector.ReportInserted(TraceTypes.Log, 1);
        }

        public override void Handle(Metric input)
        {
            var sql = BuildSql(new OneEnumerable<Metric>(input));
            DatabaseSelector.UsingDatabaseResult(TraceTypes.Metric, sql, static (res, sql) =>
            {
                res.Database.Execute(sql);
            });
            DatabaseSelector.ReportInserted(TraceTypes.Metric, 1);
        }

        public override void Handle(in Batch<Activity> inputs)
        {
            if (inputs.Count == 0)
            {
                return;
            }
            using (var enu = inputs.GetEnumerator())
            {
                var sql = BuildSql(enu);
                DatabaseSelector.UsingDatabaseResult(TraceTypes.Activity, sql, static (res, sql) =>
                {
                    res.Database.Execute(sql);
                });
                DatabaseSelector.ReportInserted(TraceTypes.Activity, (int)inputs.Count);
            }
        }

        public override void Handle(in Batch<LogRecord> inputs)
        {
            if (inputs.Count == 0)
            {
                return;
            }
            using (var enu = inputs.GetEnumerator())
            {
                var sql = BuildSql(enu);
                DatabaseSelector.UsingDatabaseResult(TraceTypes.Log, sql, static (res, sql) =>
                {
                    res.Database.Execute(sql);
                });
                DatabaseSelector.ReportInserted(TraceTypes.Log, (int)inputs.Count);
            }
        }

        public override void Handle(in Batch<Metric> inputs)
        {
            if (inputs.Count == 0)
            {
                return;
            }
            using (var enu = inputs.GetEnumerator())
            {
                var sql = BuildSql(enu);
                DatabaseSelector.UsingDatabaseResult(TraceTypes.Metric, sql, static (res, sql) =>
                {
                    res.Database.Execute(sql);
                });
                DatabaseSelector.ReportInserted(TraceTypes.Metric, (int)inputs.Count);
            }
        }

    }
}
