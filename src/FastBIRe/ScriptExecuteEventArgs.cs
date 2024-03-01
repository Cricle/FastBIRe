using System.Data.Common;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

namespace FastBIRe
{
    public readonly struct TraceUnit
    {
        public TraceUnit(StackTrace? stackTrace)
        {
            ExecutionTime = FullTime = null;
            StackTrace = stackTrace;
        }

        public TraceUnit(TimeSpan? executionTime, TimeSpan? fullTime, StackTrace? stackTrace)
        {
            ExecutionTime = executionTime;
            FullTime = fullTime;
            StackTrace = stackTrace;
        }

        public TimeSpan? ExecutionTime { get; }

        public TimeSpan? FullTime { get; }

        public StackTrace? StackTrace { get; }

        public override string ToString()
        {
            return $"ExecutionTime: {ExecutionTime}, FullTime: {FullTime}{Environment.NewLine}{StackTrace}";
        }
    }
    public readonly struct ScriptUnit
    {
        public readonly string Script;

        public readonly IEnumerable<KeyValuePair<string, object?>>? Parameters;

        public bool IsEmptyScript => DefaultScriptExecuter.IsEmptyScript(Script);

        public IQueryTranslateResult Result => QueryTranslateResult.Create(Script, Parameters);

        public ScriptUnit(string script, IEnumerable<KeyValuePair<string, object?>>? parameters=null)
        {
            Script = script;
            Parameters = parameters;
        }
        public string GetParamterString()
        {
            if (Parameters==null)
            {
                return string.Empty;
            }
            return string.Join(",", Parameters.Select(x => $"[{x.Key}={x.Value}]"));
        }

        public override string? ToString()
        {
            if (Parameters == null || !Parameters.Any())
            {
                return Script;
            }
            return $"{Script}{Environment.NewLine}{GetParamterString()}";
        }
    }

    public readonly struct ScriptExecuteEventArgs
    {
        internal ScriptExecuteEventArgs(ScriptExecutState state,
            DbConnection connection,
            DbCommand? command,
            Exception? executeException,
            int? recordsAffected,
#if !NETSTANDARD2_0
            DbBatch? batch,
            DbBatchCommand? batchCommand,
#endif
            ScriptUnit? scriptUnit,
            IEnumerable<ScriptUnit>? scriptUnits,
            TraceUnit traceUnit,
            DbTransaction? transaction)
        {
            State = state;
            Connection = connection;
            Command = command;
            ScriptUnit = scriptUnit;
            if (scriptUnit != null)
            {
                ScriptUnits = scriptUnits ?? new OneEnumerable<ScriptUnit>(scriptUnit.Value);
            }
            ExecuteException = executeException;
            RecordsAffected = recordsAffected;
#if !NETSTANDARD2_0
            Batch = batch;
            BatchCommand = batchCommand;
#endif
            TraceUnit = traceUnit;
            Transaction = transaction;
        }
        public ScriptExecutState State { get; }

        public DbConnection Connection { get; }

        public DbCommand? Command { get; }

        public ScriptUnit? ScriptUnit { get; }

        public IEnumerable<ScriptUnit>? ScriptUnits { get; }

        public Exception? ExecuteException { get; }

        public int? RecordsAffected { get; }

#if !NETSTANDARD2_0
        public DbBatch? Batch { get; }

        public DbBatchCommand? BatchCommand { get; }
#endif

        public TraceUnit? TraceUnit { get; }

        public DbTransaction? Transaction { get; }

        private static string? GetTypeName(object? value)
        {
            switch (value)
            {
                case bool:
                    return "System.Boolean";
                case byte:
                    return "System.Byte";
                case sbyte:
                    return "System.SByte";
                case char:
                    return "System.Char";
                case short:
                    return "System.Int16";
                case ushort:
                    return "System.UInt16";
                case int:
                    return "System.Int32";
                case uint:
                    return "System.UInt32";
                case long:
                    return "System.Int64";
                case ulong:
                    return "System.UInt64";
                case float:
                    return "System.Single";
                case double:
                    return "System.Double";
                case decimal:
                    return "System.Decimal";
                case DateTime:
                    return "System.DateTime";
                case DateTimeOffset:
                    return "System.DateTimeOffset";
                case Guid:
                    return "System.Guid";
                case byte[]:
                    return "System.Byte[]";
#if NET6_0_OR_GREATER
                case DateOnly:
                    return "System.DateOnly";
                case TimeOnly:
                    return "System.TimeOnly";
#endif
                case string:
                    return "System.String";
                case null:
                default:
                    return "NULL";
            }
        }

        public void ToExecuteString(StringBuilder s)
        {
            if (State == ScriptExecutState.EndReading)
            {
                s.Append("Read ");
            }
            else
            {
                s.Append("Executed ");
            }
            s.Append('(');
            if (TraceUnit!.Value.ExecutionTime == null)
            {
                s.Append('0');
            }
            else
            {
                s.Append(TraceUnit!.Value.ExecutionTime.Value.TotalMilliseconds.ToString("F2"));
            }
            s.Append("ms)T(");
            if (TraceUnit!.Value.FullTime == null)
            {
                s.Append('0');
            }
            else
            {
                s.Append(TraceUnit!.Value.FullTime.Value.TotalMilliseconds.ToString("F2"));
            }
            s.Append("ms) ");
            if (ScriptUnit != null)
            {
                s.Append('[');
                if (ScriptUnit!.Value.Parameters != null)
                {
                    s.Append("Pars=");
                    s.Append(string.Join(",", ScriptUnit!.Value.Parameters.Select(x => $"[{x.Key}='{x.Value}'({GetTypeName(x.Value)})]")));
                    s.Append(", ");
                }
                if (Command != null)
                {
                    s.Append("Timeout=");
                    s.Append(Command.CommandTimeout);
                }
                s.AppendLine("]");
                s.AppendLine(ScriptUnit!.Value.Script);
            }
        }
        public string ToExecuteString()
        {
            var s = new StringBuilder();
            ToExecuteString(s);
            return s.ToString();
        }
        public string ToExceptionString()
        {
            var s = new StringBuilder();
            ToExecuteString(s);
            s.AppendLine(ExecuteException?.ToString());
            return s.ToString();
        }
        public string? ToKnowString()
        {
            TryToKnowString(out var s);
            return s;
        }

        public string GetScriptDebugString()
        {
            if (ScriptUnit == null && ScriptUnits == null)
            {
                return string.Empty;
            }
            var s = new StringBuilder();
            if (ScriptUnit!=null)
            {
                s.AppendLine(ScriptUnit!.Value.Script);
                s.AppendLine(ArgsAsString(ScriptUnit!.Value.Parameters));
            }
            else
            {
                foreach (var item in ScriptUnits!)
                {
                    s.AppendLine(item.Script);
                    s.AppendLine(ArgsAsString(item.Parameters));
                    s.AppendLine();
                }
            }

            return s.ToString();
        }

        private string ArgsAsString(IEnumerable<KeyValuePair<string, object?>>? args)
        {
            if (args == null)
            {
                return string.Empty;
            }
            return string.Join(",", args.Select(x => $"[{x.Key}={x.Key}]"));
        }

        public bool TryToKnowString(out string? msg)
        {
            if (State == ScriptExecutState.Executed || State == ScriptExecutState.EndReading)
            {
                msg = ToExecuteString();
                return true;
            }
            else if (State == ScriptExecutState.Exception)
            {
                msg = ToExceptionString();
                return true;
            }
            msg = null;
            return false;
        }
        public static ScriptExecuteEventArgs Begin(DbConnection connection, IEnumerable<ScriptUnit> scriptUnits, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Begin,
                connection,
                null,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif                
                null,
                scriptUnits,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs Begin(DbConnection connection, ScriptUnit scriptUnit, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Begin,
                connection,
                null,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif                
                scriptUnit,
                null,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs CreatedCommand(DbConnection connection, DbCommand command, ScriptUnit scriptUnit, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.CreatedCommand,
                connection,
                command,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                scriptUnit,
                null,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs LoadCommand(DbConnection connection, DbCommand command, ScriptUnit scriptUnit, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.LoaedCommand,
                connection,
                command,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                scriptUnit,
                null,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs LoadCommand(DbConnection connection, DbCommand command, IEnumerable<ScriptUnit> scriptUnits, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.LoaedCommand,
                connection,
                command,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                null,
                scriptUnits,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs Executed(DbConnection connection, DbCommand command, IEnumerable<ScriptUnit> scriptUnits, int recordsAffected, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Executed,
                connection,
                command,
                null,
                recordsAffected,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                null,
                scriptUnits,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs Executed(DbConnection connection, DbCommand command, ScriptUnit scriptUnit, int recordsAffected, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Executed,
                connection,
                command,
                null,
                recordsAffected,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                scriptUnit,
                null,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs Exception(DbConnection connection, DbCommand? command, ScriptUnit scriptUnit, Exception exception, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Exception,
                connection,
                command,
                exception,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                scriptUnit,
                null,
                traceUnit,
                dbTransaction);
        }
#if !NETSTANDARD2_0

        public static ScriptExecuteEventArgs Exception(DbConnection connection,
            DbBatch dbBatch, IEnumerable<ScriptUnit> scriptUnits, Exception exception, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Exception,
                connection,
                null,
                exception,
                null,
                dbBatch,
                null,
                null,
                scriptUnits,
                traceUnit,
                dbTransaction);
        }
#endif
        public static ScriptExecuteEventArgs Exception(DbConnection connection, DbCommand command
            , IEnumerable<ScriptUnit> scriptUnits, Exception exception, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Exception,
                connection,
                command,
                exception,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                null,
                scriptUnits,
                traceUnit,
                dbTransaction);
        }
#if !NETSTANDARD2_0
        public static ScriptExecuteEventArgs CreatedBatch(DbConnection connection, IEnumerable<ScriptUnit> scriptUnits, DbBatch batch, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.CreatedBatch,
                connection,
                null,
                null,
                null,
                batch,
                null,
                null,
                scriptUnits,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs LoadBatchItem(DbConnection connection, IEnumerable<ScriptUnit> scriptUnits, DbBatch batch, DbBatchCommand command, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.LoadBatchItem,
                connection,
                null,
                null,
                null,
                batch,
                command,
                null,
                scriptUnits,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs ExecutedBatch(DbConnection connection, IEnumerable<ScriptUnit> scriptUnits, DbBatch batch, TraceUnit traceUnit, int? recordsAffected, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.ExecutedBatch,
                connection,
                null,
                null,
                recordsAffected,
                batch,
                null,
                null,
                scriptUnits,
                traceUnit,
                dbTransaction);
        }
#endif
        public static ScriptExecuteEventArgs Skip(DbConnection connection, ScriptUnit scriptUnit, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Skip,
                connection,
                null,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                scriptUnit,
                null,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs Skip(DbConnection connection, IEnumerable<ScriptUnit> scriptUnits, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Skip,
                connection,
                null,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                null,
                scriptUnits,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs StartReading(DbConnection connection, DbCommand command, ScriptUnit scriptUnit, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.StartReading,
                connection,
                command,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                scriptUnit,
                null,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs EndReading(DbConnection connection, DbCommand command, ScriptUnit scriptUnit, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.EndReading,
                connection,
                command,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                scriptUnit,
                null,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs BeginTranscation(DbConnection connection, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.BeginTransaction,
                connection,
                null,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                null,
                null,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs CommitedTransaction(DbConnection connection, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.CommitedTransaction,
                connection,
                null,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                null,
                null,
                traceUnit,
                dbTransaction);
        }
        public static ScriptExecuteEventArgs RollbackedTransaction(DbConnection connection, TraceUnit traceUnit, DbTransaction? dbTransaction)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.RollbackedTransaction,
                connection,
                null,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                null,
                null,
                traceUnit,
                dbTransaction);
        }
    }
}
