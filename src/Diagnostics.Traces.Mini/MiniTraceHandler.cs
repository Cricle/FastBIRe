using Diagnostics.Generator.Core;
using Diagnostics.Traces.Mini.Exceptions;
using Diagnostics.Traces.Stores;
using FastBIRe;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics;

namespace Diagnostics.Traces.Mini
{
    public class MiniTraceHandler<TIdentity> : TraceHandlerBase<TIdentity>, IBatchOperatorHandler<TraceExceptionInfo>
        where TIdentity : IEquatable<TIdentity>
    {
        public MiniTraceHandler(IUndefinedDatabaseSelector<MiniDatabaseCreatedResult>? activityDatabaseSelector, IUndefinedDatabaseSelector<MiniDatabaseCreatedResult>? logDatabaseSelector, IUndefinedDatabaseSelector<MiniDatabaseCreatedResult>? exceptionDatabaseSelector)
        {
            ActivityDatabaseSelector = activityDatabaseSelector;
            LogDatabaseSelector = logDatabaseSelector;
            ExceptionDatabaseSelector = exceptionDatabaseSelector;
        }
        private bool writeHead;

        public bool WriteHead
        {
            get => writeHead;
            set => writeHead = value;
        }

        public IUndefinedDatabaseSelector<MiniDatabaseCreatedResult>? ActivityDatabaseSelector { get; }
        public IUndefinedDatabaseSelector<MiniDatabaseCreatedResult>? LogDatabaseSelector { get; }
        public IUndefinedDatabaseSelector<MiniDatabaseCreatedResult>? ExceptionDatabaseSelector { get; }

        private void AppendActivity(IEnumerator<Activity> enu)
        {
            if (ActivityDatabaseSelector==null)
            {
                return;
            }
            while (true)
            {
                var scopeC = 0;
                try
                {
                    ActivityDatabaseSelector.UsingDatabaseResult(enu, (res, enu) =>
                    {
                        while (enu.MoveNext())
                        {
                            if (writeHead)
                            {
                                res.Serializer.WriteHead(new TraceHeader { Count = res.Count });
                            }
                            res.Serializer.TraceHelper.WriteActivity(enu.Current, res.SaveActivityModes);
                            scopeC++;
                            res.AddCount();
                        }
                    });
                    ActivityDatabaseSelector.ReportInserted(scopeC);
                    break;
                }
                catch (MemoryMapFileBufferFullException)
                {
                    ActivityDatabaseSelector.Flush();
                }
            }
        }
        private void AppendLogs(IEnumerator<LogRecord> enu)
        {
            if (LogDatabaseSelector == null)
            {
                return;
            }
            while (true)
            {
                var scopeC = 0;
                try
                {
                    LogDatabaseSelector.UsingDatabaseResult(enu, (res, enu) =>
                    {
                        while (enu.MoveNext())
                        {
                            if (writeHead)
                            {
                                res.Serializer.WriteHead(new TraceHeader { Count = res.Count });
                            }
                            res.Serializer.TraceHelper.WriteLog(enu.Current, res.SaveLogModes);
                            scopeC++;
                            res.AddCount();
                        }
                    });
                    LogDatabaseSelector.ReportInserted(scopeC);
                    break;
                }
                catch (MemoryMapFileBufferFullException)
                {
                    LogDatabaseSelector.Flush();
                }
            }
        }
        private void AppendExceptions(IEnumerator<TraceExceptionInfo> enu)
        {
            if (ExceptionDatabaseSelector == null)
            {
                return;
            }
            while (true)
            {
                var scopeC = 0;
                try
                {
                    ExceptionDatabaseSelector.UsingDatabaseResult(enu, (res, enu) =>
                    {
                        while (enu.MoveNext())
                        {
                            if (writeHead)
                            {
                                res.Serializer.WriteHead(new TraceHeader { Count = res.Count });
                            }
                            res.Serializer.TraceHelper.WriteException(enu.Current, res.SaveExceptionModes);
                            scopeC++;
                            res.AddCount();
                        }
                    });
                    ExceptionDatabaseSelector.ReportInserted(scopeC);
                    break;
                }
                catch (MemoryMapFileBufferFullException)
                {
                    ExceptionDatabaseSelector.Flush();
                }
            }
        }

        public override void Handle(Activity input)
        {
            AppendActivity(new OneEnumerable<Activity>(input));
        }

        public override void Handle(LogRecord input)
        {
            AppendLogs(new OneEnumerable<LogRecord>(input));
        }

        public override void Handle(Metric input)
        {
            throw new NotImplementedException();
        }

        public override void Handle(in Batch<Activity> inputs)
        {
            using (var enu=inputs.GetEnumerator())
            {
                AppendActivity(enu);
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
            throw new NotImplementedException();
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
