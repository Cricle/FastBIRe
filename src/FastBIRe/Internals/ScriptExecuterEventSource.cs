using Diagnostics.Generator.Core;
using Diagnostics.Generator.Core.Annotations;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace FastBIRe.Internals
{
    [EventSourceGenerate]
    [EventSource(Name = EventName, Guid = "64770723-C48E-40CA-87AF-70472E2A2C95")]
    [MapToActivity(typeof(ScriptExecuterActivity),WithEventSourceCall =true)]
    internal sealed partial class ScriptExecuterEventSource : EventSource
    {
        public const string EventName = "FastBIRe.ScriptExecuter";

#if !NETSTANDARD2_0
        [Counter("total-execute-count", CounterTypes.PollingCounter, DisplayName = "Execute command count (Total)")]
        private long totalExecute;
        [Counter("total-execute-fail", CounterTypes.PollingCounter, DisplayName = "Execute fail count (Total)")]
        private long totalFail;
        [Counter("total-read-count", CounterTypes.PollingCounter, DisplayName = "Read count (Total)")]
        private long totalRead;
        [Counter("total-commit-transaction-count", CounterTypes.PollingCounter, DisplayName = "Transaction commit count (Total)")]
        private long totalCommitTransaction;
        [Counter("total-rollback-transaction-count", CounterTypes.PollingCounter, DisplayName = "Transaction rollback count (Total)")]
        private long totalRollbackTranscation;

        [Counter("executed-time", CounterTypes.IncrementingEventCounter, DisplayName = "Executed time", DisplayUnits = "ms", DisplayRateTimeScaleMs = 1000)]
        private IncrementingEventCounter? executedTime;
        [Counter("executed-full-time", CounterTypes.IncrementingEventCounter, DisplayName = "Executed full time", DisplayUnits = "ms", DisplayRateTimeScaleMs = 1000)]
        private IncrementingEventCounter? executedFullTime;
        [Counter("read-time", CounterTypes.IncrementingEventCounter, DisplayName = "Read time", DisplayUnits = "ms", DisplayRateTimeScaleMs = 1000)]
        private IncrementingEventCounter? readTime;
        [Counter("read-full-time", CounterTypes.IncrementingEventCounter, DisplayName = "Read full time", DisplayUnits = "ms", DisplayRateTimeScaleMs = 1000)]
        private IncrementingEventCounter? readFullTime;
#endif

        private const EventKeywords KeyWords = EventKeywords.MicrosoftTelemetry | EventKeywords.EventLogClassic;

        [EventSourceAccesstorInstance]
        public static readonly ScriptExecuterEventSource Instance = new ScriptExecuterEventSource();

        private bool IsEnableStackTrace => IsEnabled(EventLevel.Verbose, EventKeywords.All);

        [Event(1, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteBegin(string? connectionString, string? database, string? script, string? stackTrace);
        [Event(2, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteCreatedCommand(bool inTrans);
        [Event(3, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteLoadCommand(string? args, int timeout);
        [Event(4, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteExecuted(double executeTime, double fullTime, int recordsAffected);
        [Event(5, Level = EventLevel.Error, Channel = EventChannel.Analytic, Keywords = KeyWords)]
        public unsafe partial void WriteException(double executeTime, double fullTime, string? exception);
        [Event(6, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteCreateBatch(bool inTrans);
        [Event(7, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteLoadBatch();
        [Event(8, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteExecutedBatch(int timeout, double executeTime, double fullTime, int recordsAffected);
        [Event(10, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteSkip();
        [Event(11, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteStartReading();
        [Event(12, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteEndReading(double executeTime, double fullTime);
        [Event(13, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteBeginTranscation(string? connectionString, string? database, string? script, bool inTrans, double executeTime, double fullTime, string? stackTrace);
        [Event(14, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteCommitedTransaction(double executeTime, double fullTime);
        [Event(15, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteRollbackedTransaction(double executeTime, double fullTime);

        [NonEvent]
        public void WriteScriptExecuteEventArgs(in ScriptExecuteEventArgs e)
        {
            var stackTrace = IsEnableStackTrace ? e.TraceUnit?.StackTrace?.ToString() : null;
            switch (e.State)
            {
                case ScriptExecutState.Begin:
                    ScriptExecuterActivity.WriteBegin(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), stackTrace);
                    break;
                case ScriptExecutState.CreatedCommand:
                    ScriptExecuterActivity.WriteCreatedCommand(e.Transaction != null);
                    break;
                case ScriptExecutState.LoaedCommand:
                    ScriptExecuterActivity.WriteLoadCommand(e.ScriptUnit?.GetParamterString(), e.Command!.CommandTimeout);
                    break;
                case ScriptExecutState.Executed:
                    ScriptExecuterActivity.WriteExecuted(e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, e.RecordsAffected ?? 0);
#if !NETSTANDARD2_0
                    IncrementTotalExecute();
                    if (executedTime != null && e.TraceUnit?.ExecutionTime != null)
                    {
                        executedTime.Increment(e.TraceUnit.Value.ExecutionTime.Value.TotalMilliseconds);
                    }
                    if (executedFullTime != null && e.TraceUnit?.FullTime != null)
                    {
                        executedFullTime.Increment(e.TraceUnit.Value.FullTime.Value.TotalMilliseconds);
                    }
#endif
                    break;
                case ScriptExecutState.ExecutedBatch:
                    ScriptExecuterActivity.WriteExecutedBatch(e.Command!.CommandTimeout, e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, e.RecordsAffected ?? 0);
                    break;
                case ScriptExecutState.CreatedBatch:
                    ScriptExecuterActivity.WriteCreateBatch(e.Transaction != null);
                    break;
                case ScriptExecutState.LoadBatchItem:
                    ScriptExecuterActivity.WriteLoadBatch();
                    break;
                case ScriptExecutState.Exception:
                    ScriptExecuterActivity.WriteException(e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, e.ExecuteException?.ToString());
#if !NETSTANDARD2_0
                    IncrementTotalFail();
#endif
                    Activity.Current?.SetStatus(ActivityStatusCode.Error, e.ExecuteException?.Message);
                    break;
                case ScriptExecutState.Skip:
                    ScriptExecuterActivity.WriteSkip();
                    break;
                case ScriptExecutState.StartReading:
                    ScriptExecuterActivity.WriteStartReading();
                    break;
                case ScriptExecutState.EndReading:
                    ScriptExecuterActivity.WriteEndReading(e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0);
#if !NETSTANDARD2_0
                    IncrementTotalRead();
                    if (readTime != null && e.TraceUnit?.ExecutionTime != null)
                    {
                        readTime.Increment(e.TraceUnit.Value.ExecutionTime.Value.TotalMilliseconds);
                    }
                    if (readFullTime != null && e.TraceUnit?.FullTime != null)
                    {
                        readFullTime.Increment(e.TraceUnit.Value.FullTime.Value.TotalMilliseconds);
                    }
#endif
                    break;
                case ScriptExecutState.BeginTransaction:
                    ScriptExecuterActivity.WriteBeginTranscation(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Transaction != null, e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, stackTrace);
                    break;
                case ScriptExecutState.CommitedTransaction:
                    ScriptExecuterActivity.WriteCommitedTransaction(e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0);
#if !NETSTANDARD2_0
                    IncrementTotalCommitTransaction();
#endif
                    break;
                case ScriptExecutState.RollbackedTransaction:
                    ScriptExecuterActivity.WriteRollbackedTransaction(e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0);
#if !NETSTANDARD2_0
                    IncrementTotalRollbackTranscation();
#endif
                    break;
                default:
                    break;
            }
        }
    }
}
