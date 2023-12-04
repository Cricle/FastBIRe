using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace FastBIRe.Internals.Etw
{
    [EventSource(Name = "FastBIRe.ScriptExecuter", Guid = "64770723-C48E-40CA-87AF-70472E2A2C95")]
    internal sealed unsafe partial class ScriptExecuterEventSource : EtwEventSource
    {
        private const EventKeywords KeyWords = EventKeywords.MicrosoftTelemetry | EventKeywords.EventLogClassic;

        public static readonly ScriptExecuterEventSource Instance = new ScriptExecuterEventSource();

        private bool IsEnableStackTrace => IsEnabled(EventLevel.Verbose, EventKeywords.All);

        [Event(1, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteBegin(string? connectionString, string? database, string? script, string? stackTrace, string? args)
        {
            EventData* datas = stackalloc EventData[5];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);
            WriteString(&datas[3], stackTrace);
            WriteString(&datas[4], args);
            WriteEventCore(1, 5, datas);
        }
        [Event(2, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteCreatedCommand(string? connectionString, string? database, string? script, string? stackTrace, string? args)
        {
            EventData* datas = stackalloc EventData[4];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);
            WriteString(&datas[3], stackTrace);
            WriteEventCore(2, 4, datas);
        }
        [Event(3, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteLoadCommand(string? connectionString, string? database, string? script, int timeout, bool inTrans, string? stackTrace, string? args)
        {
            EventData* datas = stackalloc EventData[6];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);
            int s = inTrans ? 1 : 0;
            datas[3].DataPointer = (IntPtr)(&timeout);
            datas[3].Size = sizeof(int);
            datas[4].DataPointer = (IntPtr)(&s);
            datas[4].Size = sizeof(int);
            WriteString(&datas[5], stackTrace);
            WriteEventCore(3, 6, datas);
        }
        [Event(4, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteExecuted(string? connectionString, string? database, string? script, int timeout, bool inTrans, double executeTime, double fullTime, int recordsAffected, string? stackTrace, string? args)
        {
            var aff = recordsAffected < 0 ? 0 : recordsAffected;
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[10];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);
            datas[3].DataPointer = (IntPtr)(&timeout);
            datas[3].Size = sizeof(int);

            datas[4].DataPointer = (IntPtr)(&aff);
            datas[4].Size = sizeof(int);

            datas[5].DataPointer = (IntPtr)(&executeTime);
            datas[5].Size = sizeof(double);

            datas[6].DataPointer = (IntPtr)(&fullTime);
            datas[6].Size = sizeof(double);

            datas[7].DataPointer = (IntPtr)(&s);
            datas[7].Size = sizeof(int);
            WriteString(&datas[8], stackTrace);
            WriteString(&datas[9], args);

            WriteEventCore(4, 10, datas);
        }
        [Event(5, Level = EventLevel.Error, Channel = EventChannel.Analytic, Keywords = KeyWords)]
        public void WriteException(string? connectionString, string? database, string? script, int timeout, bool inTrans, double executeTime, double fullTime, string? stackTrans, string? exception, string? args)
        {
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[10];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);
            datas[3].DataPointer = (IntPtr)(&timeout);
            datas[3].Size = sizeof(int);

            WriteString(&datas[4], stackTrans);

            datas[5].DataPointer = (IntPtr)(&executeTime);
            datas[5].Size = sizeof(double);

            datas[6].DataPointer = (IntPtr)(&fullTime);
            datas[6].Size = sizeof(double);

            datas[7].DataPointer = (IntPtr)(&s);
            datas[7].Size = sizeof(int);

            WriteString(&datas[8], exception);
            WriteString(&datas[9], args);

            WriteEventCore(5, 10, datas);
        }
        [Event(6, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteCreateBatch(string? connectionString, string? database, string? script, bool inTrans, string? stackTrace, string? args)
        {
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[6];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);

            datas[3].DataPointer = (IntPtr)(&s);
            datas[3].Size = sizeof(int);
            WriteString(&datas[4], stackTrace);
            WriteString(&datas[5], args);

            WriteEventCore(6, 6, datas);
        }
        [Event(7, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteLoadBatch(string? connectionString, string? database, string? script, bool inTrans, string? stackTrace, string? args)
        {
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[6];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);

            datas[3].DataPointer = (IntPtr)(&s);
            datas[3].Size = sizeof(int);

            WriteString(&datas[4], stackTrace);
            WriteString(&datas[5], args);

            WriteEventCore(7, 6, datas);
        }
        [Event(8, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteExecutedBatch(string? connectionString, string? database, string? script, int timeout, bool inTrans, double executeTime, double fullTime, int recordsAffected, string? stackTrace, string? args)
        {
            var aff = recordsAffected < 0 ? 0 : recordsAffected;
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[10];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);
            datas[3].DataPointer = (IntPtr)(&timeout);
            datas[3].Size = sizeof(int);

            datas[4].DataPointer = (IntPtr)(&aff);
            datas[4].Size = sizeof(int);

            datas[5].DataPointer = (IntPtr)(&executeTime);
            datas[5].Size = sizeof(double);

            datas[6].DataPointer = (IntPtr)(&fullTime);
            datas[6].Size = sizeof(double);

            datas[7].DataPointer = (IntPtr)(&s);
            datas[7].Size = sizeof(int);

            WriteString(&datas[8], script);
            WriteString(&datas[9], args);
            WriteEventCore(8, 10, datas);
        }
        [Event(9, Level = EventLevel.Error, Channel = EventChannel.Analytic, Keywords = KeyWords)]
        public void WriteBatchException(string? connectionString, string? database, string? script, int timeout, bool inTrans, double executeTime, double fullTime, string? stackTrans, string? exception, string? args)
        {
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[10];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);
            datas[3].DataPointer = (IntPtr)(&timeout);
            datas[3].Size = sizeof(int);

            WriteString(&datas[4], stackTrans);

            datas[5].DataPointer = (IntPtr)(&executeTime);
            datas[5].Size = sizeof(double);

            datas[6].DataPointer = (IntPtr)(&fullTime);
            datas[6].Size = sizeof(double);

            datas[7].DataPointer = (IntPtr)(&s);
            datas[7].Size = sizeof(int);

            WriteString(&datas[8], exception);
            WriteString(&datas[9], args);

            WriteEventCore(9, 10, datas);
        }
        [Event(10, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteSkip(string? connectionString, string? database, string? script, bool inTrans, string? stackTrace, string? args)
        {
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[6];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);

            datas[3].DataPointer = (IntPtr)(&s);
            datas[3].Size = sizeof(int);

            WriteString(&datas[4], stackTrace);
            WriteString(&datas[5], args);
            WriteEventCore(10, 6, datas);
        }
        [Event(11, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteStartReading(string? connectionString, string? database, string? script, bool inTrans, int timeout, string? stackTrace, string? args)
        {
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[7];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);

            datas[3].DataPointer = (IntPtr)(&s);
            datas[3].Size = sizeof(int);

            datas[4].DataPointer = (IntPtr)(&timeout);
            datas[4].Size = sizeof(int);

            WriteString(&datas[5], stackTrace);
            WriteString(&datas[6], args);
            WriteEventCore(11, 7, datas);
        }
        [Event(12, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteEndReading(string? connectionString, string? database, string? script, bool inTrans, double executeTime, double fullTime, string? stackTrace, string? args)
        {
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[8];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);

            datas[3].DataPointer = (IntPtr)(&executeTime);
            datas[3].Size = sizeof(double);

            datas[4].DataPointer = (IntPtr)(&fullTime);
            datas[4].Size = sizeof(double);

            datas[5].DataPointer = (IntPtr)(&s);
            datas[5].Size = sizeof(int);

            WriteString(&datas[6], stackTrace);
            WriteString(&datas[7], args);
            WriteEventCore(12, 8, datas);
        }
        [Event(13, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteBeginTranscation(string? connectionString, string? database, string? script, bool inTrans, double executeTime, double fullTime, string? stackTrace)
        {
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[7];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);

            datas[3].DataPointer = (IntPtr)(&executeTime);
            datas[3].Size = sizeof(double);

            datas[4].DataPointer = (IntPtr)(&fullTime);
            datas[4].Size = sizeof(double);

            datas[5].DataPointer = (IntPtr)(&s);
            datas[5].Size = sizeof(int);

            WriteString(&datas[6], stackTrace);
            WriteEventCore(13, 7, datas);
        }
        [Event(14, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteCommitedTransaction(string? connectionString, string? database, string? script, bool inTrans, double executeTime, double fullTime, string? stackTrace)
        {
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[7];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);

            datas[3].DataPointer = (IntPtr)(&executeTime);
            datas[3].Size = sizeof(double);

            datas[4].DataPointer = (IntPtr)(&fullTime);
            datas[4].Size = sizeof(double);

            datas[5].DataPointer = (IntPtr)(&s);
            datas[5].Size = sizeof(int);

            WriteString(&datas[6], script);
            WriteEventCore(14, 7, datas);
        }
        [Event(15, Channel = EventChannel.Analytic, Keywords = KeyWords, Level = EventLevel.Informational)]
        public void WriteRollbackedTransaction(string? connectionString, string? database, string? script, bool inTrans, double executeTime, double fullTime, string? stackTrace)
        {
            var s = inTrans ? 1 : 0;
            EventData* datas = stackalloc EventData[7];
            WriteString(&datas[0], connectionString);
            WriteString(&datas[1], database);
            WriteString(&datas[2], script);

            datas[3].DataPointer = (IntPtr)(&executeTime);
            datas[3].Size = sizeof(double);

            datas[4].DataPointer = (IntPtr)(&fullTime);
            datas[4].Size = sizeof(double);

            datas[5].DataPointer = (IntPtr)(&s);
            datas[5].Size = sizeof(int);

            WriteString(&datas[6], script);
            WriteEventCore(15, 7, datas);
        }
        [NonEvent]
        public void WriteScriptExecuteEventArgs(in ScriptExecuteEventArgs e)
        {
            var stackTrace = IsEnableStackTrace ? e.StackTrace?.ToString() : null;
            switch (e.state)
            {
                case ScriptExecutState.Begin:
                    WriteBegin(e.Connection.ConnectionString, e.Connection.Database, e.Scripts?.FirstOrDefault(), stackTrace, AsString(e.Argss));
                    break;
                case ScriptExecutState.CreatedCommand:
                    WriteCreatedCommand(e.Connection.ConnectionString, e.Connection.Database, e.Scripts?.FirstOrDefault(), stackTrace, AsString(e.Argss));
                    break;
                case ScriptExecutState.LoaedCommand:
                    WriteLoadCommand(e.Connection.ConnectionString, e.Connection.Database, e.Scripts?.FirstOrDefault(), e.Command!.CommandTimeout, e.Transaction != null, stackTrace, AsString(e.Argss));
                    break;
                case ScriptExecutState.Executed:
                    WriteExecuted(e.Connection.ConnectionString, e.Connection.Database, e.Scripts?.FirstOrDefault(), e.Command!.CommandTimeout, e.Transaction != null, e.ExecutionTime?.TotalMilliseconds ?? 0, e.FullTime?.TotalMilliseconds ?? 0, e.RecordsAffected ?? 0, stackTrace, AsString(e.Argss));
                    break;
                case ScriptExecutState.ExecutedBatch:
                    WriteExecutedBatch(e.Connection.ConnectionString, e.Connection.Database, ToString(e.Scripts), e.Command!.CommandTimeout, e.Transaction != null, e.ExecutionTime?.TotalMilliseconds ?? 0, e.FullTime?.TotalMilliseconds ?? 0, e.RecordsAffected ?? 0, stackTrace, AsString(e.Argss));
                    break;
                case ScriptExecutState.CreatedBatch:
                    WriteCreateBatch(e.Connection.ConnectionString, e.Connection.Database, ToString(e.Scripts), e.Transaction != null, stackTrace, AsString(e.Argss));
                    break;
                case ScriptExecutState.LoadBatchItem:
                    WriteLoadBatch(e.Connection.ConnectionString, e.Connection.Database, ToString(e.Scripts), e.Transaction != null, stackTrace, AsString(e.Argss));
                    break;
                case ScriptExecutState.Exception:
                    WriteException(e.Connection.ConnectionString, e.Connection.Database, e.Scripts?.FirstOrDefault(), e.Command?.CommandTimeout ?? 0, e.Transaction != null, e.ExecutionTime?.TotalMilliseconds ?? 0, e.FullTime?.TotalMilliseconds ?? 0, e.StackTrace?.ToString(), e.ExecuteException?.ToString(), AsString(e.Argss));
                    break;
                case ScriptExecutState.BatchException:
                    WriteBatchException(e.Connection.ConnectionString, e.Connection.Database, ToString(e.Scripts), e.Command?.CommandTimeout ?? 0, e.Transaction != null, e.ExecutionTime?.TotalMilliseconds ?? 0, e.FullTime?.TotalMilliseconds ?? 0, e.StackTrace?.ToString(), e.ExecuteException?.ToString(), AsString(e.Argss));
                    break;
                case ScriptExecutState.Skip:
                    WriteSkip(e.Connection.ConnectionString, e.Connection.Database, ToString(e.Scripts), e.Transaction != null, stackTrace, AsString(e.Argss));
                    break;
                case ScriptExecutState.StartReading:
                    WriteStartReading(e.Connection.ConnectionString, e.Connection.Database, e.Scripts?.FirstOrDefault(), e.Transaction != null, e.Command?.CommandTimeout ?? 0, stackTrace, AsString(e.Argss));
                    break;
                case ScriptExecutState.EndReading:
                    WriteEndReading(e.Connection.ConnectionString, e.Connection.Database, e.Scripts?.FirstOrDefault(), e.Transaction != null, e.ExecutionTime?.TotalMilliseconds ?? 0, e.FullTime?.TotalMilliseconds ?? 0, stackTrace, AsString(e.Argss));
                    break;
                case ScriptExecutState.BeginTransaction:
                    WriteBeginTranscation(e.Connection.ConnectionString, e.Connection.Database, e.Scripts?.FirstOrDefault(), e.Transaction != null, e.ExecutionTime?.TotalMilliseconds ?? 0, e.FullTime?.TotalMilliseconds ?? 0, stackTrace);
                    break;
                case ScriptExecutState.CommitedTransaction:
                    WriteCommitedTransaction(e.Connection.ConnectionString, e.Connection.Database, e.Scripts?.FirstOrDefault(), e.Transaction != null, e.ExecutionTime?.TotalMilliseconds ?? 0, e.FullTime?.TotalMilliseconds ?? 0, stackTrace);
                    break;
                case ScriptExecutState.RollbackedTransaction:
                    WriteRollbackedTransaction(e.Connection.ConnectionString, e.Connection.Database, e.Scripts?.FirstOrDefault(), e.Transaction != null, e.ExecutionTime?.TotalMilliseconds ?? 0, e.FullTime?.TotalMilliseconds ?? 0, stackTrace);
                    break;
                default:
                    break;
            }
        }
    }
}
