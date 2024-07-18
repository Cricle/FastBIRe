using Diagnostics.Generator.Core;
using Diagnostics.Traces.Models;
using Diagnostics.Traces.Stores;
using FastBIRe;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using ParquetSharp;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ValueBuffer;

namespace Diagnostics.Traces.Parquet
{
    public class ParquetTraceHandler<TIdentity> : TraceHandlerBase<TIdentity>, IBatchOperatorHandler<TraceExceptionInfo>
        where TIdentity : IEquatable<TIdentity>
    {
        public ParquetTraceHandler(IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult>? activityDatabaseSelector,
            IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult>? logsDatabaseSelector,
            IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult>? exceptionsDatabaseSelector,
            IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult>? metricDatabaseSelector,
            IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult>? exceptionDatabaseSelector,
            IIdentityProvider<TIdentity, Activity>? activityIdentityProvider,
            IIdentityProvider<TIdentity, LogRecord>? logIdentityProvider,
            IIdentityProvider<TIdentity, Metric>? metricIdentityProvider)
        {
            ActivityDatabaseSelector = activityDatabaseSelector;
            LogsDatabaseSelector = logsDatabaseSelector;
            ExceptionsDatabaseSelector = exceptionsDatabaseSelector;
            MetricDatabaseSelector = metricDatabaseSelector;
            ExceptionDatabaseSelector = exceptionDatabaseSelector;

            ActivityIdentityProvider = activityIdentityProvider;
            LogIdentityProvider = logIdentityProvider;
            MetricIdentityProvider = metricIdentityProvider;
        }

        public IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult>? ActivityDatabaseSelector { get; }
        public IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult>? LogsDatabaseSelector { get; }
        public IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult>? ExceptionsDatabaseSelector { get; }
        public IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult>? MetricDatabaseSelector { get; }
        public IUndefinedDatabaseSelector<ParquetDatabaseCreatedResult>? ExceptionDatabaseSelector { get; }

        public IIdentityProvider<TIdentity, Activity>? ActivityIdentityProvider { get; }

        public IIdentityProvider<TIdentity, LogRecord>? LogIdentityProvider { get; }

        public IIdentityProvider<TIdentity, Metric>? MetricIdentityProvider { get; }

        private void AppendLogs(IEnumerator<LogRecord> logs)
        {
            if (LogsDatabaseSelector == null)
            {
                return;
            }
            var mode = LogsDatabaseSelector.UnsafeUsingDatabaseResult(static x => x.SaveLogModes);

            using var timestamps = new ValueList<DateTime>();
            using var logLevels = new ValueList<int>();
            using var categoryNames = new ValueList<string?>();
            using var traceIds = new ValueList<string?>();
            using var spanIds = new ValueList<string?>();
            using var attributes = new ValueList<string?>();
            using var formattedMessages = new ValueList<string?>();
            using var bodys = new ValueList<string?>();

            while (logs.MoveNext())
            {
                var item = logs.Current;
                if (LogIdentityProvider != null && !LogIdentityProvider.GetIdentity(item).Succeed)
                {
                    continue;
                }

                if ((mode & SaveLogModes.Timestamp) != 0)
                {
                    timestamps.Add(item.Timestamp);
                }
                if ((mode & SaveLogModes.LogLevel) != 0)
                {
                    logLevels.Add((int)item.LogLevel);
                }
                if ((mode & SaveLogModes.CategoryName) != 0)
                {
                    categoryNames.Add(item.CategoryName);
                }
                if ((mode & SaveLogModes.TraceId) != 0)
                {
                    if (item.TraceId.Equals(default))
                    {
                        traceIds.Add((string?)null);
                    }
                    else
                    {
                        traceIds.Add(item.TraceId.ToString());
                    }
                }
                if ((mode & SaveLogModes.SpanId) != 0)
                {
                    if (item.SpanId.Equals(default))
                    {
                        spanIds.Add((string?)null);
                    }
                    else
                    {
                        spanIds.Add(item.SpanId.ToString());
                    }
                }
                if ((mode & SaveLogModes.Attributes) != 0)
                {
                    attributes.Add(JsonSerializer.Serialize(item.Attributes, AttributeJsonConverter.Options));
                }
                if ((mode & SaveLogModes.FormattedMessage) != 0)
                {
                    formattedMessages.Add(item.FormattedMessage);
                }
                if ((mode & SaveLogModes.Body) != 0)
                {
                    bodys.Add(item.Body);
                }
            }
            LogsDatabaseSelector.UsingDatabaseResult(res =>
            {
                using (var writer = res.GetWriter())
                using (var appender = writer.Operator.AppendBufferedRowGroup())
                {
                    var idx = 0;
                    if (timestamps.Size != 0)
                        WriteColumn(in timestamps, appender.Column(idx++).LogicalWriter<DateTime>());
                    if (logLevels.Size != 0)
                        WriteColumn(in logLevels, appender.Column(idx++).LogicalWriter<int>());
                    if (categoryNames.Size != 0)
                        WriteColumn(in categoryNames, appender.Column(idx++).LogicalWriter<string?>());
                    if (traceIds.Size != 0)
                        WriteColumn(in traceIds, appender.Column(idx++).LogicalWriter<string?>());
                    if (spanIds.Size != 0)
                        WriteColumn(in spanIds, appender.Column(idx++).LogicalWriter<string?>());
                    if (attributes.Size != 0)
                        WriteColumn(in attributes, appender.Column(idx++).LogicalWriter<string?>());
                    if (formattedMessages.Size != 0)
                        WriteColumn(in formattedMessages, appender.Column(idx++).LogicalWriter<string?>());
                    if (bodys.Size != 0)
                        WriteColumn(in bodys, appender.Column(idx++).LogicalWriter<string?>());
                }
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteColumn<T>(in ValueList<T> datas, LogicalColumnWriter<T> writer)
        {
            using (writer)
            {
                for (int i = 0; i < datas.BufferSlotIndex; i++)
                {
                    var span = datas.GetSlot(i);
                    writer.WriteBatch(span);
                }
            }
        }
        private void AppendMetrics(IEnumerator<Metric> metrics)
        {
            if (MetricDatabaseSelector == null)
            {
                return;
            }

            using var name = new ValueList<string?>();
            using var unit = new ValueList<string?>();
            using var metricType = new ValueList<int>();
            using var temporality = new ValueList<byte>();
            using var description = new ValueList<string?>();
            using var meterName = new ValueList<string?>();
            using var meterVersion = new ValueList<string?>();
            using var meterTags = new ValueList<string?>();
            using var createTime = new ValueList<DateTime>();
            using var points = new ValueList<string?>();

            var now = DateTime.Now;
            while (metrics.MoveNext())
            {
                var item = metrics.Current;
                if (MetricIdentityProvider != null && !MetricIdentityProvider.GetIdentity(item).Succeed)
                {
                    continue;
                }

                name.Add(item.Name);
                unit.Add(item.Unit);
                metricType.Add((int)item.MetricType);
                temporality.Add((byte)item.Temporality);
                description.Add(item.Description);
                meterName.Add(item.MeterName);
                meterVersion.Add(item.MeterVersion);
                meterTags.Add(JsonSerializer.Serialize(item.MeterTags, TagsJsonConverter.Options));
                createTime.Add(now);
                points.Add(JsonSerializer.Serialize(item, MetricPointsAccessorJsonConverter.Options));

                MetricDatabaseSelector.UsingDatabaseResult(res =>
                {
                    using (var writer = res.GetWriter())
                    using (var appender = writer.Operator.AppendBufferedRowGroup())
                    {
                        var idx = 0;
                        WriteColumn(in name, appender.Column(idx++).LogicalWriter<string?>());
                        WriteColumn(in unit, appender.Column(idx++).LogicalWriter<string?>());
                        WriteColumn(in metricType, appender.Column(idx++).LogicalWriter<int>());
                        WriteColumn(in temporality, appender.Column(idx++).LogicalWriter<byte>());
                        WriteColumn(in description, appender.Column(idx++).LogicalWriter<string?>());
                        WriteColumn(in meterName, appender.Column(idx++).LogicalWriter<string?>());
                        WriteColumn(in meterVersion, appender.Column(idx++).LogicalWriter<string?>());
                        WriteColumn(in meterTags, appender.Column(idx++).LogicalWriter<string?>());
                        WriteColumn(in createTime, appender.Column(idx++).LogicalWriter<DateTime>());
                        WriteColumn(in points, appender.Column(idx++).LogicalWriter<string?>());
                    }
                });
            }
        }
        private void AppendExceptions(IEnumerator<TraceExceptionInfo> exceptions)
        {
            if (ExceptionDatabaseSelector == null)
            {
                return;
            }
            var mode = ExceptionDatabaseSelector.UnsafeUsingDatabaseResult(static x => x.SaveExceptionModes);

            using var traceId = new ValueList<string?>();
            using var spanId = new ValueList<string?>();
            using var createTime = new ValueList<DateTime>();
            using var typeName = new ValueList<string?>();
            using var message = new ValueList<string?>();
            using var helpLink = new ValueList<string?>();
            using var hResult = new ValueList<int>();
            using var stackTrace = new ValueList<string?>();
            using var innerException = new ValueList<string?>();

            while (exceptions.MoveNext())
            {
                var item = exceptions.Current;
                if (mode.HasFlag(SaveExceptionModes.TraceId))
                {
                    if (item.TraceId.Equals(default))
                    {
                        traceId.Add((string?)null);
                    }
                    else
                    {
                        traceId.Add(item.TraceId.ToString());
                    }
                }
                if (mode.HasFlag(SaveExceptionModes.SpanId))
                {
                    if (item.TraceId.Equals(default))
                    {
                        spanId.Add((string?)null);
                    }
                    else
                    {
                        spanId.Add(item.SpanId.ToString());
                    }
                }
                if (mode.HasFlag(SaveExceptionModes.CreateTime))
                {
                    createTime.Add(item.CreateTime);
                }
                if (mode.HasFlag(SaveExceptionModes.TypeName))
                {
                    typeName.Add(item.Exception.GetType().FullName);
                }
                if (mode.HasFlag(SaveExceptionModes.Message))
                {
                    message.Add(item.Exception.Message);
                }
                if (mode.HasFlag(SaveExceptionModes.HelpLink))
                {
                    helpLink.Add(item.Exception.HelpLink);
                }
                if (mode.HasFlag(SaveExceptionModes.HResult))
                {
                    hResult.Add(item.Exception.HResult);
                }
                if (mode.HasFlag(SaveExceptionModes.StackTrace))
                {
                    stackTrace.Add(item.Exception.StackTrace);
                }
                if (mode.HasFlag(SaveExceptionModes.InnerException))
                {
                    innerException.Add(item.Exception.InnerException?.ToString());
                }

                ExceptionDatabaseSelector.UsingDatabaseResult(res =>
                {
                    using (var writer = res.GetWriter())
                    using (var appender = writer.Operator.AppendBufferedRowGroup())
                    {
                        var idx = 0;
                        if (traceId.Size != 0)
                            WriteColumn(in traceId, appender.Column(idx++).LogicalWriter<string?>());
                        if (spanId.Size != 0)
                            WriteColumn(in spanId, appender.Column(idx++).LogicalWriter<string?>());
                        if (createTime.Size != 0)
                            WriteColumn(in createTime, appender.Column(idx++).LogicalWriter<DateTime>());
                        if (typeName.Size != 0)
                            WriteColumn(in typeName, appender.Column(idx++).LogicalWriter<string?>());
                        if (message.Size != 0)
                            WriteColumn(in message, appender.Column(idx++).LogicalWriter<string?>());
                        if (helpLink.Size != 0)
                            WriteColumn(in helpLink, appender.Column(idx++).LogicalWriter<string?>());
                        if (hResult.Size != 0)
                            WriteColumn(in hResult, appender.Column(idx++).LogicalWriter<int>());
                        if (stackTrace.Size != 0)
                            WriteColumn(in stackTrace, appender.Column(idx++).LogicalWriter<string?>());
                        if (innerException.Size != 0)
                            WriteColumn(in innerException, appender.Column(idx++).LogicalWriter<string?>());
                    }
                });
            }
        }
        private void AppendActivities(IEnumerator<Activity> activities)
        {
            if (ActivityDatabaseSelector == null)
            {
                return;
            }
            var mode = ActivityDatabaseSelector.UnsafeUsingDatabaseResult(static x => x.SaveActivityModes);

            using var ids = new ValueList<string?>();
            using var status = new ValueList<int>();
            using var statusDescription = new ValueList<string?>();
            using var hasRemoteParent = new ValueList<bool>();
            using var kind = new ValueList<int>();
            using var operationName = new ValueList<string?>();
            using var displayName = new ValueList<string?>();
            using var sourceName = new ValueList<string?>();
            using var sourceVersion = new ValueList<string?>();
            using var duration = new ValueList<double>();
            using var startTimeUtc = new ValueList<DateTime>();
            using var parentId = new ValueList<string?>();
            using var rootId = new ValueList<string?>();
            using var tags = new ValueList<string?>();
            using var events = new ValueList<string?>();
            using var links = new ValueList<string?>();
            using var baggage = new ValueList<string?>();
            using var context = new ValueList<string?>();
            using var traceStateString = new ValueList<string?>();
            using var spanId = new ValueList<string?>();
            using var traceId = new ValueList<string?>();
            using var recorded = new ValueList<bool>();
            using var activityTraceFlags = new ValueList<int>();
            using var parentSpanId = new ValueList<string?>();

            while (activities.MoveNext())
            {
                var item = activities.Current;
                if (ActivityIdentityProvider != null && !ActivityIdentityProvider.GetIdentity(item).Succeed)
                {
                    continue;
                }

                if ((mode & SaveActivityModes.Id) != 0)
                {
                    ids.Add(item.Id);
                }
                if ((mode & SaveActivityModes.Status) != 0)
                {
                    status.Add((int)item.Status);
                }
                if ((mode & SaveActivityModes.StatusDescription) != 0)
                {
                    statusDescription.Add(item.StatusDescription);
                }
                if ((mode & SaveActivityModes.HasRemoteParent) != 0)
                {
                    hasRemoteParent.Add(item.HasRemoteParent);
                }
                if ((mode & SaveActivityModes.Kind) != 0)
                {
                    kind.Add((int)item.Kind);

                }
                if ((mode & SaveActivityModes.OperationName) != 0)
                {
                    operationName.Add(item.OperationName);

                }
                if ((mode & SaveActivityModes.DisplayName) != 0)
                {
                    displayName.Add(item.DisplayName);

                }
                if ((mode & SaveActivityModes.SourceName) != 0)
                {
                    sourceName.Add(item.Source.Name);

                }
                if ((mode & SaveActivityModes.SourceVersion) != 0)
                {
                    sourceVersion.Add(item.Source.Version);

                }
                if ((mode & SaveActivityModes.Duration) != 0)
                {
                    duration.Add(item.Duration.TotalMilliseconds);

                }
                if ((mode & SaveActivityModes.StartTimeUtc) != 0)
                {
                    startTimeUtc.Add(item.StartTimeUtc);

                }
                if ((mode & SaveActivityModes.ParentId) != 0)
                {
                    parentId.Add(item.OperationName);

                }
                if ((mode & SaveActivityModes.RootId) != 0)
                {
                    rootId.Add(item.RootId);

                }
                if ((mode & SaveActivityModes.Tags) != 0)
                {
                    tags.Add(JsonSerializer.Serialize(item.Tags, TagsJsonConverter.Options));

                }
                if ((mode & SaveActivityModes.Events) != 0)
                {
                    events.Add(JsonSerializer.Serialize(item.Events, ActivityEventJsonConverter.Options));
                }
                if ((mode & SaveActivityModes.Links) != 0)
                {
                    links.Add(JsonSerializer.Serialize(item.Links, ActivityLinksJsonConverter.Options));
                }
                if ((mode & SaveActivityModes.Baggage) != 0)
                {
                    baggage.Add(JsonSerializer.Serialize(item.Baggage, TagsJsonConverter.Options));
                }
                if ((mode & SaveActivityModes.Context) != 0)
                {
                    baggage.Add(JsonSerializer.Serialize(item.Context, ActivityContextJsonConverter.Options));
                }
                if ((mode & SaveActivityModes.TraceStateString) != 0)
                {
                    traceStateString.Add(item.TraceStateString);
                }
                if ((mode & SaveActivityModes.SpanId) != 0)
                {
                    traceStateString.Add(item.SpanId.ToString());
                }
                if ((mode & SaveActivityModes.TraceId) != 0)
                {
                    traceId.Add(item.TraceId.ToString());
                }
                if ((mode & SaveActivityModes.Recorded) != 0)
                {
                    recorded.Add(item.Recorded);
                }
                if ((mode & SaveActivityModes.ActivityTraceFlags) != 0)
                {
                    activityTraceFlags.Add((int)item.ActivityTraceFlags);
                }
                if ((mode & SaveActivityModes.ParentSpanId) != 0)
                {
                    if (item.ParentSpanId.Equals(default))
                    {
                        parentSpanId.Add((string?)null);
                    }
                    else
                    {
                        parentSpanId.Add(item.ParentSpanId.ToString());
                    }
                }
            }
            ActivityDatabaseSelector.UsingDatabaseResult(res =>
            {
                using (var writer = res.GetWriter())
                using (var appender = writer.Operator.AppendBufferedRowGroup())
                {
                    var idx = 0;
                    if (ids.Size != 0)
                        WriteColumn(in ids, appender.Column(idx++).LogicalWriter<string?>());
                    if (status.Size != 0)
                        WriteColumn(in status, appender.Column(idx++).LogicalWriter<int>());
                    if (statusDescription.Size != 0)
                        WriteColumn(in statusDescription, appender.Column(idx++).LogicalWriter<string?>());
                    if (hasRemoteParent.Size != 0)
                        WriteColumn(in hasRemoteParent, appender.Column(idx++).LogicalWriter<bool>());
                    if (kind.Size != 0)
                        WriteColumn(in kind, appender.Column(idx++).LogicalWriter<int>());
                    if (operationName.Size != 0)
                        WriteColumn(in operationName, appender.Column(idx++).LogicalWriter<string?>());
                    if (displayName.Size != 0)
                        WriteColumn(in displayName, appender.Column(idx++).LogicalWriter<string?>());
                    if (sourceName.Size != 0)
                        WriteColumn(in sourceName, appender.Column(idx++).LogicalWriter<string?>());
                    if (sourceVersion.Size != 0)
                        WriteColumn(in sourceVersion, appender.Column(idx++).LogicalWriter<string?>());
                    if (duration.Size != 0)
                        WriteColumn(in duration, appender.Column(idx++).LogicalWriter<double>());
                    if (startTimeUtc.Size != 0)
                        WriteColumn(in startTimeUtc, appender.Column(idx++).LogicalWriter<DateTime>());
                    if (parentId.Size != 0)
                        WriteColumn(in parentId, appender.Column(idx++).LogicalWriter<string?>());
                    if (rootId.Size != 0)
                        WriteColumn(in rootId, appender.Column(idx++).LogicalWriter<string?>());
                    if (tags.Size != 0)
                        WriteColumn(in tags, appender.Column(idx++).LogicalWriter<string?>());
                    if (events.Size != 0)
                        WriteColumn(in events, appender.Column(idx++).LogicalWriter<string?>());
                    if (links.Size != 0)
                        WriteColumn(in links, appender.Column(idx++).LogicalWriter<string?>());
                    if (baggage.Size != 0)
                        WriteColumn(in baggage, appender.Column(idx++).LogicalWriter<string?>());
                    if (context.Size != 0)
                        WriteColumn(in context, appender.Column(idx++).LogicalWriter<string?>());
                    if (traceStateString.Size != 0)
                        WriteColumn(in traceStateString, appender.Column(idx++).LogicalWriter<string?>());
                    if (spanId.Size != 0)
                        WriteColumn(in spanId, appender.Column(idx++).LogicalWriter<string?>());
                    if (traceId.Size != 0)
                        WriteColumn(in traceId, appender.Column(idx++).LogicalWriter<string?>());
                    if (recorded.Size != 0)
                        WriteColumn(in recorded, appender.Column(idx++).LogicalWriter<bool>());
                    if (activityTraceFlags.Size != 0)
                        WriteColumn(in activityTraceFlags, appender.Column(idx++).LogicalWriter<int>());
                    if (parentSpanId.Size != 0)
                        WriteColumn(in parentSpanId, appender.Column(idx++).LogicalWriter<string?>());
                }
            });
        }
        public override void Handle(Activity input)
        {
            AppendActivities(new OneEnumerable<Activity>(input));
        }

        public override void Handle(LogRecord input)
        {
            AppendLogs(new OneEnumerable<LogRecord>(input));
        }

        public override void Handle(Metric input)
        {
            AppendMetrics(new OneEnumerable<Metric>(input));
        }

        public override void Handle(in Batch<Activity> inputs)
        {
            using (var enu = inputs.GetEnumerator())
            {
                AppendActivities(enu);
            }
        }

        public override void Handle(in Batch<LogRecord> inputs)
        {
            using (var enu = inputs.GetEnumerator())
            {
                AppendLogs(enu);
            }
        }

        public override void Handle(in Batch<Metric> inputs)
        {
            using (var enu = inputs.GetEnumerator())
            {
                AppendMetrics(enu);
            }
        }

        public Task HandleAsync(BatchData<TraceExceptionInfo> inputs, CancellationToken token)
        {
            using (var enu = inputs.GetEnumerator())
            {
                AppendExceptions(enu);
            }
            return Task.CompletedTask;
        }
    }
}
