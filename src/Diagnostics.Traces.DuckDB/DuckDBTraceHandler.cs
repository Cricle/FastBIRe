using Diagnostics.Generator.Core;
using Diagnostics.Traces.Stores;
using FastBIRe;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

        private void AppendLogRecord(IEnumerator<LogRecord> logs)
        {
            var count = 0;
            DatabaseSelector.UsingDatabaseResult(res =>
            {
                var mode = res.SaveLogModes;
                using (var appender = res.Connection.CreateAppender("logs"))
                {
                    while (logs.MoveNext())
                    {
                        if (LogIdentityProvider != null && !LogIdentityProvider.GetIdentity(logs.Current).Succeed)
                        {
                            continue;
                        }
                        var row = appender.CreateRow();
                        if ((mode & SaveLogModes.Timestamp) != 0)
                        {
                            row.AppendValue(logs.Current.Timestamp);
                        }
                        if ((mode & SaveLogModes.LogLevel) != 0)
                        {
                            row.AppendValue((short)logs.Current.LogLevel);
                        }
                        if ((mode & SaveLogModes.CategoryName) != 0)
                        {
                            row.AppendValue(logs.Current.CategoryName);
                        }
                        if ((mode & SaveLogModes.TraceId) != 0)
                        {
                            row.AppendValue(logs.Current.TraceId.ToString());
                        }
                        if ((mode & SaveLogModes.SpanId) != 0)
                        {
                            row.AppendValue(logs.Current.SpanId.ToString());
                        }
                        if ((mode & SaveLogModes.FormattedMessage) != 0)
                        {
                            row.AppendValue(logs.Current.FormattedMessage);
                        }
                        if ((mode & SaveLogModes.Body) != 0)
                        {
                            row.AppendValue(logs.Current.Body);
                        }
                        row.EndRow();
                        count++;
                    }
                }
            });
            DatabaseSelector.ReportInserted(count);
        }

        private ValueStringBuilder? BuildSql(IEnumerator<LogRecord> logs)
        {
            var mode = DatabaseSelector.UnsafeUsingDatabaseResult(static x => x.SaveLogModes);
            var s = new ValueStringBuilder();
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
                if ((mode & SaveLogModes.Timestamp) != 0)
                {
                    s.Append(DuckHelper.WrapValue(item.Timestamp));
                    s.Append(',');
                }
                if ((mode & SaveLogModes.LogLevel) != 0)
                {
                    s.Append(DuckHelper.WrapValue(item.LogLevel));
                    s.Append(',');
                }
                if ((mode & SaveLogModes.CategoryName) != 0)
                {
                    s.Append(DuckHelper.WrapValue(item.CategoryName));
                    s.Append(',');
                }
                if ((mode & SaveLogModes.TraceId) != 0)
                {
                    s.Append(DuckHelper.WrapValue(item.TraceId.ToString()));
                    s.Append(',');
                }
                if ((mode & SaveLogModes.SpanId) != 0)
                {
                    s.Append(DuckHelper.WrapValue(item.SpanId.ToString()));
                    s.Append(',');
                }
                if ((mode & SaveLogModes.Attributes) != 0)
                {
                    s.Append(DuckHelper.WrapValue(item.Attributes));
                    s.Append(',');
                }
                if ((mode & SaveLogModes.FormattedMessage) != 0)
                {
                    s.Append(DuckHelper.WrapValue(item.FormattedMessage));
                    s.Append(',');
                }
                if ((mode & SaveLogModes.Body) != 0)
                {
                    s.Append(DuckHelper.WrapValue(item.Body));
                    s.Append(',');
                }
                s._chars.RemoveLast(1);
                s.Append("),");
            }
            if (!any)
            {
                return null;
            }
            s._chars.RemoveLast(1);
            //s.Remove(s.Length - 1, 1);
            return s;
        }
        private ValueStringBuilder? BuildSql(IEnumerator<Activity> activities)
        {
            var mode = DatabaseSelector.UnsafeUsingDatabaseResult(static x => x.SaveActivityModes);
            var s = new ValueStringBuilder();
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
                if ((mode & SaveActivityModes.Id) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Id));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Status) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Status));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.StatusDescription) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.StatusDescription));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.HasRemoteParent) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.HasRemoteParent));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Kind) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Kind));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.OperationName) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.OperationName));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.DisplayName) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.DisplayName));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.SourceName) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Source.Name));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.SourceVersion) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Source.Version));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Duration) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Duration.TotalMilliseconds));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.StartTimeUtc) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.StartTimeUtc));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.ParentId) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.ParentId));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.RootId) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.RootId));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Tags) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Tags));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Events) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Events));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Links) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Links));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Baggage) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Baggage));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Context) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Context));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.TraceStateString) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.TraceStateString));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.SpanId) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.SpanId.ToString()));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.TraceId) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.TraceId.ToString()));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Recorded) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.Recorded));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.ActivityTraceFlags) != 0)
                {

                    s.Append(DuckHelper.WrapValue(item.ActivityTraceFlags));
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.ParentSpanId) != 0)
                {
                    if (item.ParentSpanId.Equals(default))
                    {
                        s.Append("NULL");
                    }
                    else
                    {
                        s.Append(DuckHelper.WrapValue(item.ParentSpanId.ToString()));
                    }
                    s.Append(',');
                }
                s._chars.RemoveLast(1);
                s.Append("),");
            }
            if (!any)
            {
                return null;
            }
            s._chars.RemoveLast(1);
            //s.Remove(s.Length - 1, 1);
            return s;
        }
        private void AppendExceptionRecord(IEnumerator<TraceExceptionInfo> ex)
        {
            var count = 0;
            DatabaseSelector.UsingDatabaseResult(res =>
            {
                var mode = res.SaveExceptionModes;
                using (var appender = res.Connection.CreateAppender("exceptions"))
                {
                    while (ex.MoveNext())
                    {
                        var row = appender.CreateRow();
                        if (mode.HasFlag(SaveExceptionModes.TraceId))
                        {
                            row.AppendValue(ex.Current.TraceId.ToString());
                        }
                        if (mode.HasFlag(SaveExceptionModes.SpanId))
                        {
                            row.AppendValue(ex.Current.SpanId.ToString());
                        }
                        if (mode.HasFlag(SaveExceptionModes.CreateTime))
                        {
                            row.AppendValue(ex.Current.CreateTime);
                        }
                        if (mode.HasFlag(SaveExceptionModes.TypeName))
                        {
                            row.AppendValue(ex.Current.Exception.GetType().FullName);
                        }
                        if (mode.HasFlag(SaveExceptionModes.Message))
                        {
                            row.AppendValue(ex.Current.Exception.Message);
                        }
                        if (mode.HasFlag(SaveExceptionModes.HelpLink))
                        {
                            row.AppendValue(ex.Current.Exception.HelpLink);
                        }
                        if (mode.HasFlag(SaveExceptionModes.HResult))
                        {
                            row.AppendValue(ex.Current.Exception.HResult);
                        }
                        if (mode.HasFlag(SaveExceptionModes.StackTrace))
                        {
                            row.AppendValue(ex.Current.Exception.StackTrace);
                        }
                        if (mode.HasFlag(SaveExceptionModes.InnerException))
                        {
                            row.AppendValue(ex.Current.Exception.InnerException?.ToString());
                        }
                        row.EndRow();
                        count++;
                    }
                }
            });
            DatabaseSelector.ReportInserted(count);
        }
        private ValueStringBuilder? BuildSql(in BatchData<TraceExceptionInfo> exceptions)
        {
            var s = new ValueStringBuilder();
            var mode = DatabaseSelector.UnsafeUsingDatabaseResult(static x => x.SaveExceptionModes);
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
                if (mode.HasFlag(SaveExceptionModes.TraceId))
                {
                    s.Append(DuckHelper.WrapValue(item.TraceId?.ToString()));
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.SpanId))
                {

                    s.Append(DuckHelper.WrapValue(item.SpanId?.ToString()));
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.CreateTime))
                {

                    s.Append(DuckHelper.WrapValue(item.CreateTime));
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.TypeName))
                {

                    s.Append(DuckHelper.WrapValue(item.Exception.GetType().FullName));
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.Message))
                {

                    s.Append(DuckHelper.WrapValue(item.Exception.Message));
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.HelpLink))
                {

                    s.Append(DuckHelper.WrapValue(item.Exception.HelpLink));
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.HResult))
                {

                    s.Append(DuckHelper.WrapValue(item.Exception.HResult));
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.Data))
                {

                    s.Append(DuckHelper.WrapValue(item.Exception.Data));
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.StackTrace))
                {

                    s.Append(DuckHelper.WrapValue(item.Exception.StackTrace));
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.InnerException))
                {

                    s.Append(DuckHelper.WrapValue(item.Exception.InnerException?.ToString()));
                    s.Append(',');
                }
                s._chars.RemoveLast(1);
                s.Append(')');
            }

            return s;
        }
        private ValueStringBuilder? BuildSql(IEnumerator<Metric> metrics)
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
                    return null;
                }
                //s.Remove(s.Length - 1, 1);
                return s;
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
            using var sql = BuildSql(new OneEnumerable<Activity>(input));
            if (sql != null)
            {
                DatabaseSelector.UsingDatabaseResult(sql.Value, static (res, sql) =>
                {
                    DuckDBNativeHelper.DuckDBQuery(res.NativeConnection, sql);
                });
                DatabaseSelector.ReportInserted(1);
            }
        }

        public override void Handle(LogRecord input)
        {
            if (LogIdentityProvider != null && !LogIdentityProvider.GetIdentity(input).Succeed)
            {
                return;
            }
            var mode = DatabaseSelector.UnsafeUsingDatabaseResult(static x => x.SaveLogModes);
            if (mode.HasFlag(SaveLogModes.Attributes))
            {
                using var sql = BuildSql(new OneEnumerable<LogRecord>(input));
                if (sql != null)
                {
                    DatabaseSelector.UsingDatabaseResult(sql.Value, static (res, sql) =>
                    {
                        DuckDBNativeHelper.DuckDBQuery(res.NativeConnection, sql);
                    });
                    DatabaseSelector.ReportInserted(1);
                }
            }
            else
            {
                AppendLogRecord(new OneEnumerable<LogRecord>(input));
            }
        }

        public override void Handle(Metric input)
        {
            if (MetricIdentityProvider != null && !MetricIdentityProvider.GetIdentity(input).Succeed)
            {
                return;
            }
            using var sql = BuildSql(new OneEnumerable<Metric>(input));
            if (sql != null)
            {
                DatabaseSelector.UsingDatabaseResult(sql.Value, static (res, sql) =>
                {
                    DuckDBNativeHelper.DuckDBQuery(res.NativeConnection, sql);
                });
                DatabaseSelector.ReportInserted(1);
            }
        }

        public override void Handle(in Batch<Activity> inputs)
        {
            if (inputs.Count == 0)
            {
                return;
            }
            using (var enu = inputs.GetEnumerator())
            {
                using var sql = BuildSql(enu);
                if (sql != null)
                {
                    DatabaseSelector.UsingDatabaseResult(sql.Value, static (res, sql) =>
                    {
                        DuckDBNativeHelper.DuckDBQuery(res.NativeConnection, sql);
                    });
                    DatabaseSelector.ReportInserted((int)inputs.Count);
                }
            }
        }

        public override void Handle(in Batch<LogRecord> inputs)
        {
            if (inputs.Count == 0)
            {
                return;
            }
            var mode = DatabaseSelector.UnsafeUsingDatabaseResult(static x => x.SaveLogModes);

            using (var enu = inputs.GetEnumerator())
            {
                if (mode.HasFlag(SaveLogModes.Attributes))
                {
                    using var sql = BuildSql(enu);
                    if (sql != null)
                    {
                        DatabaseSelector.UsingDatabaseResult(sql.Value, static (res, sql) => DuckDBNativeHelper.DuckDBQuery(res.NativeConnection, sql));
                        DatabaseSelector.ReportInserted((int)inputs.Count);
                    }
                }
                else
                {
                    AppendLogRecord(enu);
                }
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
                using var sql = BuildSql(enu);
                if (sql != null)
                {
                    DatabaseSelector.UsingDatabaseResult(sql.Value, static (res, sql) =>
                    {
                        DuckDBNativeHelper.DuckDBQuery(res.NativeConnection, sql);
                    });
                    DatabaseSelector.ReportInserted((int)inputs.Count);
                }
            }
        }

        public Task HandleAsync(BatchData<TraceExceptionInfo> inputs, CancellationToken token)
        {
            if (inputs.Count == 0)
            {
                return Task.CompletedTask;
            }
            var mode = DatabaseSelector.UnsafeUsingDatabaseResult(static x => x.SaveExceptionModes);
            if (!mode.HasFlag(SaveExceptionModes.Data))
            {
                using (var enu = inputs.GetEnumerator())
                {
                    AppendExceptionRecord(enu);
                }
            }
            using var sql = BuildSql(inputs);
            if (sql != null)
            {
                DatabaseSelector.UsingDatabaseResult(sql.Value, static (res, sql) =>
                {
                    DuckDBNativeHelper.DuckDBQuery(res.NativeConnection, sql);
                });
                DatabaseSelector.ReportInserted(inputs.Count);
            }
            return Task.CompletedTask;
        }
    }
}
