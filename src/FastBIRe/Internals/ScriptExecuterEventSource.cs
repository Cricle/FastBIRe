using Diagnostics.Generator.Core;
using Diagnostics.Generator.Core.Annotations;
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
        public unsafe partial void WriteCreatedCommand(string? connectionString, string? database, string? script, string? stackTrace);
        [Event(3, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteLoadCommand(string? connectionString, string? database, string? script, int timeout, bool inTrans, string? stackTrace);
        [Event(4, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteExecuted(string? connectionString, string? database, string? script, int timeout, bool inTrans, double executeTime, double fullTime, int recordsAffected, string? stackTrace);
        [Event(5, Level = EventLevel.Error, Channel = EventChannel.Analytic, Keywords = KeyWords)]
        public unsafe partial void WriteException(string? connectionString, string? database, string? script, int timeout, bool inTrans, double executeTime, double fullTime, string? stackTrans, string? exception);
        [Event(6, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteCreateBatch(string? connectionString, string? database, string? script, bool inTrans, string? stackTrace);
        [Event(7, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteLoadBatch(string? connectionString, string? database, string? script, bool inTrans, string? stackTrace);
        [Event(8, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteExecutedBatch(string? connectionString, string? database, string? script, int timeout, bool inTrans, double executeTime, double fullTime, int recordsAffected, string? stackTrace);
        [Event(10, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteSkip(string? connectionString, string? database, string? script, bool inTrans, string? stackTrace);
        [Event(11, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteStartReading(string? connectionString, string? database, string? script, bool inTrans, int timeout, string? stackTrace);
        [Event(12, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteEndReading(string? connectionString, string? database, string? script, bool inTrans, double executeTime, double fullTime, string? stackTrace);
        [Event(13, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteBeginTranscation(string? connectionString, string? database, string? script, bool inTrans, double executeTime, double fullTime, string? stackTrace);
        [Event(14, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteCommitedTransaction(string? connectionString, string? database, string? script, bool inTrans, double executeTime, double fullTime, string? stackTrace);
        [Event(15, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public unsafe partial void WriteRollbackedTransaction(string? connectionString, string? database, string? script, bool inTrans, double executeTime, double fullTime, string? stackTrace);

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
                    WriteCreatedCommand(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), stackTrace);
                    break;
                case ScriptExecutState.LoaedCommand:
                    WriteLoadCommand(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Command!.CommandTimeout, e.Transaction != null, stackTrace);
                    break;
                case ScriptExecutState.Executed:
                    WriteExecuted(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Command!.CommandTimeout, e.Transaction != null, e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, e.RecordsAffected ?? 0, stackTrace);
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
                    WriteExecutedBatch(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Command!.CommandTimeout, e.Transaction != null, e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, e.RecordsAffected ?? 0, stackTrace);
                    break;
                case ScriptExecutState.CreatedBatch:
                    WriteCreateBatch(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Transaction != null, stackTrace);
                    break;
                case ScriptExecutState.LoadBatchItem:
                    WriteLoadBatch(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Transaction != null, stackTrace);
                    break;
                case ScriptExecutState.Exception:
                    WriteException(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Command?.CommandTimeout ?? 0, e.Transaction != null, e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, e.TraceUnit?.StackTrace?.ToString(), e.ExecuteException?.ToString());
#if !NETSTANDARD2_0
                    IncrementTotalFail();
#endif
                    break;
                case ScriptExecutState.Skip:
                    WriteSkip(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Transaction != null, stackTrace);
                    break;
                case ScriptExecutState.StartReading:
                    WriteStartReading(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Transaction != null, e.Command?.CommandTimeout ?? 0, stackTrace);
                    break;
                case ScriptExecutState.EndReading:
                    WriteEndReading(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Transaction != null, e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, stackTrace);
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
                    WriteBeginTranscation(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Transaction != null, e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, stackTrace);
                    break;
                case ScriptExecutState.CommitedTransaction:
                    WriteCommitedTransaction(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Transaction != null, e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, stackTrace);
#if !NETSTANDARD2_0
                    IncrementTotalCommitTransaction();
#endif
                    break;
                case ScriptExecutState.RollbackedTransaction:
                    WriteRollbackedTransaction(e.Connection.ConnectionString, e.Connection.Database, e.GetScriptDebugString(), e.Transaction != null, e.TraceUnit?.ExecutionTime?.TotalMilliseconds ?? 0, e.TraceUnit?.FullTime?.TotalMilliseconds ?? 0, stackTrace);
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
