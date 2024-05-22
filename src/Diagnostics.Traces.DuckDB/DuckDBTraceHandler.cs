using Diagnostics.Generator.Core;
using Diagnostics.Traces.Stores;
using FastBIRe;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using ValueBuffer;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBTraceHandler<TIdentity> : TraceHandlerBase<TIdentity>, IBatchOperatorHandler<TraceExceptionInfo>
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
            var any = false;
            while (logs.MoveNext())
            {
                var item = logs.Current;

                if (LogIdentityProvider != null && !LogIdentityProvider.GetIdentity(item).Succeed)
                {
                    continue;
                }
                any = true;
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
            if (!any)
            {
                return string.Empty;
            }
            s._chars.RemoveLast(1);
            //s.Remove(s.Length - 1, 1);
            return s.ToString();
        }
        private string BuildSql(IEnumerator<Activity> activities)
        {
            using var s = new ValueStringBuilder();
            s.Append("INSERT INTO \"activities\" VALUES ");
            var any = false;
            while (activities.MoveNext())
            {
                var item = activities.Current;

                if (ActivityIdentityProvider != null && !ActivityIdentityProvider.GetIdentity(item).Succeed)
                {
                    continue;
                }
                any = true;
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
            if (!any)
            {
                return string.Empty;
            }
            s._chars.RemoveLast(1);
            //s.Remove(s.Length - 1, 1);
            return s.ToString();
        }
        private string BuildSql(in BatchData<TraceExceptionInfo> exceptions)
        {
            using var s = new ValueStringBuilder();
            s.Append("INSERT INTO \"exceptions\" VALUES ");
            var datas = exceptions.Datas;
            var isFirst = true;
            for (int i = 0; i < datas.Length; i++)
            {
                var item = datas[i];
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    s.Append(',');
                }
                s.Append('(');
                s.Append(DuckHelper.WrapValue(item.TraceId?.ToString()));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.SpanId?.ToString()));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.CreateTime));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Exception.GetType().FullName));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Exception.Message));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Exception.HelpLink));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Exception.HResult));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Exception.Data));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Exception.StackTrace));
                s.Append(',');
                s.Append(DuckHelper.WrapValue(item.Exception.InnerException?.ToString()));
                s.Append(')');
            }

            return s.ToString();
        }
        private string BuildSql(IEnumerator<Metric> metrics)
        {
            var s = new ValueStringBuilder();
            try
            {
                var any = false;
                s.Append("INSERT INTO \"metrics\" VALUES ");
                var isFirst = true;
                while (metrics.MoveNext())
                {
                    var item = metrics.Current;

                    if (MetricIdentityProvider != null && !MetricIdentityProvider.GetIdentity(item).Succeed)
                    {
                        continue;
                    }
                    any = true;
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        s.Append(',');
                    }
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
                    s.Append(DuckHelper.WrapValue(DateTime.Now));
                    s.Append(',');
                    DuckHelper.MapAsString(ref s, item.MetricType, item.GetMetricPoints());

                    s.Append(")");
                }
                if (!any)
                {
                    return string.Empty;
                }
                //s.Remove(s.Length - 1, 1);
                return s.ToString();
            }
            finally
            {
                s.Dispose();
            }
        }

        public override void Handle(Activity input)
        {
            if (ActivityIdentityProvider != null && !ActivityIdentityProvider.GetIdentity(input).Succeed)
            {
                return;
            }
            var sql = BuildSql(new OneEnumerable<Activity>(input));
            DatabaseSelector.UsingDatabaseResult(sql, static (res, sql) =>
            {
                res.Connection.Execute(sql);
            });
            DatabaseSelector.ReportInserted(1);
        }

        public override void Handle(LogRecord input)
        {
            if (LogIdentityProvider != null && !LogIdentityProvider.GetIdentity(input).Succeed)
            {
                return;
            }
            var sql = BuildSql(new OneEnumerable<LogRecord>(input));
            DatabaseSelector.UsingDatabaseResult(sql, static (res, sql) =>
            {
                res.Connection.Execute(sql);
            });
            DatabaseSelector.ReportInserted(1);
        }

        public override void Handle(Metric input)
        {
            if (MetricIdentityProvider != null && !MetricIdentityProvider.GetIdentity(input).Succeed)
            {
                return;
            }
            var sql = BuildSql(new OneEnumerable<Metric>(input));
            DatabaseSelector.UsingDatabaseResult(sql, static (res, sql) =>
            {
                res.Connection.Execute(sql);
            });
            DatabaseSelector.ReportInserted(1);
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
                DatabaseSelector.UsingDatabaseResult(sql, static (res, sql) =>
                {
                    res.Connection.Execute(sql);
                });
                DatabaseSelector.ReportInserted((int)inputs.Count);
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
                DatabaseSelector.UsingDatabaseResult(sql, static (res, sql) =>
                {
                    res.Connection.Execute(sql);
                });
                DatabaseSelector.ReportInserted((int)inputs.Count);
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
                DatabaseSelector.UsingDatabaseResult(sql, static (res, sql) =>
                {
                    res.Connection.Execute(sql);
                });
                DatabaseSelector.ReportInserted((int)inputs.Count);
            }
        }

        public Task HandleAsync(BatchData<TraceExceptionInfo> inputs, CancellationToken token)
        {
            if (inputs.Count == 0)
            {
                return Task.CompletedTask;
            }
            return Task.Factory.StartNew(() =>
            {
                var sql = BuildSql(inputs);
                DatabaseSelector.UsingDatabaseResult(sql, static (res, sql) =>
                {
                    res.Connection.Execute(sql);
                });
                DatabaseSelector.ReportInserted(inputs.Count);
            }, token);
        }
    }
}
