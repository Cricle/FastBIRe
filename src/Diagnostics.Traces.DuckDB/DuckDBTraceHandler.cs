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

        private void AppendLogRecord(IEnumerator<LogRecord> logs)
        {
            var c = DatabaseSelector.UsingDatabaseResult(res =>
            {
                var count = 0;
                var mode = res.SaveLogModes;
                using (var appender = res.Connection.CreateAppender("logs"))
                {
                    while (logs.MoveNext())
                    {
                        var current = logs.Current;
                        if (LogIdentityProvider != null && !LogIdentityProvider.GetIdentity(current).Succeed)
                        {
                            continue;
                        }
                        var row = appender.CreateRow();
                        if ((mode & SaveLogModes.Timestamp) != 0)
                        {
                            row.AppendValue(current.Timestamp);
                        }
                        if ((mode & SaveLogModes.LogLevel) != 0)
                        {
                            row.AppendValue((short)current.LogLevel);
                        }
                        if ((mode & SaveLogModes.CategoryName) != 0)
                        {
                            row.AppendValue(current.CategoryName);
                        }
                        if ((mode & SaveLogModes.TraceId) != 0)
                        {
                            row.AppendValue(current.TraceId.ToString());
                        }
                        if ((mode & SaveLogModes.SpanId) != 0)
                        {
                            row.AppendValue(current.SpanId.ToString());
                        }
                        if ((mode & SaveLogModes.FormattedMessage) != 0)
                        {
                            if (string.IsNullOrEmpty(current.FormattedMessage))
                            {
                                row.AppendNullValue();
                            }
                            else
                            {
                                using (var r = GzipHelper.Compress(current.FormattedMessage!))
                                {
                                    row.AppendValue(r.Span);
                                }
                            }
                        }
                        if ((mode & SaveLogModes.Body) != 0)
                        {
                            row.AppendValue(current.Body);
                        }
                        row.EndRow();
                        count++;
                    }
                }
                return count;
            });
            DatabaseSelector.ReportInserted(c);
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
                    DuckHelper.WrapValue(ref s,item.Timestamp);
                    s.Append(',');
                }
                if ((mode & SaveLogModes.LogLevel) != 0)
                {
                    DuckHelper.WrapValue(ref s,item.LogLevel);
                    s.Append(',');
                }
                if ((mode & SaveLogModes.CategoryName) != 0)
                {
                    DuckHelper.WrapValue(ref s, item.CategoryName);
                    s.Append(',');
                }
                if ((mode & SaveLogModes.TraceId) != 0)
                {
                    DuckHelper.WrapValue(ref s, item.TraceId.ToString());
                    s.Append(',');
                }
                if ((mode & SaveLogModes.SpanId) != 0)
                {
                    DuckHelper.WrapValue(ref s, item.SpanId.ToString());
                    s.Append(',');
                }
                if ((mode & SaveLogModes.Attributes) != 0)
                {
                    DuckHelper.WrapValue(ref s, item.Attributes);
                    s.Append(',');
                }
                if ((mode & SaveLogModes.FormattedMessage) != 0)
                {
                    DuckHelper.WrapValue(ref s, item.FormattedMessage);
                    s.Append(',');
                }
                if ((mode & SaveLogModes.Body) != 0)
                {
                    DuckHelper.WrapValue(ref s, item.Body);
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

                    DuckHelper.WrapValue(ref s, item.Id);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Status) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Status);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.StatusDescription) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.StatusDescription);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.HasRemoteParent) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.HasRemoteParent);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Kind) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Kind);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.OperationName) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.OperationName);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.DisplayName) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.DisplayName);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.SourceName) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Source.Name);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.SourceVersion) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Source.Version);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Duration) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Duration.TotalMilliseconds);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.StartTimeUtc) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.StartTimeUtc);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.ParentId) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.ParentId);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.RootId) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.RootId);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Tags) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Tags);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Events) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Events);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Links) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Links);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Baggage) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Baggage);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Context) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Context);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.TraceStateString) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.TraceStateString);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.SpanId) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.SpanId.ToString());
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.TraceId) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.TraceId.ToString());
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.Recorded) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.Recorded);
                    s.Append(',');
                }
                if ((mode & SaveActivityModes.ActivityTraceFlags) != 0)
                {

                    DuckHelper.WrapValue(ref s, item.ActivityTraceFlags);
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
                        DuckHelper.WrapValue(ref s, item.ParentSpanId.ToString());
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
                            row.AppendValue(ex.Current.TraceId);
                        }
                        if (mode.HasFlag(SaveExceptionModes.SpanId))
                        {
                            row.AppendValue(ex.Current.SpanId);
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
                    DuckHelper.WrapValue(ref s, item.TraceId?.ToString());
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.SpanId))
                {

                   DuckHelper.WrapValue(ref s, item.SpanId?.ToString());
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.CreateTime))
                {

                    DuckHelper.WrapValue(ref s, item.CreateTime);
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.TypeName))
                {

                   DuckHelper.WrapValue(ref s, item.Exception.GetType().FullName);
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.Message))
                {

                    DuckHelper.WrapValue(ref s, item.Exception.Message);
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.HelpLink))
                {

                    DuckHelper.WrapValue(ref s, item.Exception.HelpLink);
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.HResult))
                {

                    DuckHelper.WrapValue(ref s, item.Exception.HResult);
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.Data))
                {

                    DuckHelper.WrapValue(ref s, item.Exception.Data);
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.StackTrace))
                {

                    DuckHelper.WrapValue(ref s, item.Exception.StackTrace);
                    s.Append(',');
                }
                if (mode.HasFlag(SaveExceptionModes.InnerException))
                {

                    DuckHelper.WrapValue(ref s, item.Exception.InnerException?.ToString());
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
                    DuckHelper.WrapValue(ref s, item.Name);
                    s.Append(',');
                    DuckHelper.WrapValue(ref s, item.Unit);
                    s.Append(',');
                    DuckHelper.WrapValue(ref s, item.MetricType);
                    s.Append(',');
                    DuckHelper.WrapValue(ref s, item.Temporality);
                    s.Append(',');
                    DuckHelper.WrapValue(ref s, item.Description);
                    s.Append(',');
                    DuckHelper.WrapValue(ref s, item.MeterName);
                    s.Append(',');
                    DuckHelper.WrapValue(ref s, item.MeterVersion);
                    s.Append(',');
                    DuckHelper.WrapValue(ref s, item.MeterTags);
                    s.Append(',');
                    DuckHelper.WrapValue(ref s, DateTime.Now);
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
