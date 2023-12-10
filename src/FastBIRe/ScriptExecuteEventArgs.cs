using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastBIRe
{
    [EventData]
    public readonly unsafe partial struct ScriptExecuteEventArgs
    {
        public ScriptExecuteEventArgs(ScriptExecutState state,
            DbConnection connection,
            DbCommand? command,
            IEnumerable<string>? scripts,
            Exception? executeException,
            int? recordsAffected,
#if !NETSTANDARD2_0
            DbBatch? batch,
            DbBatchCommand? batchCommand,
#endif
            TimeSpan? executionTime,
            TimeSpan? fullTime,
            StackTrace? stackTrace,
            IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss,
            IEnumerable<KeyValuePair<string, object?>>? args,
            DbTransaction? transaction,
            CancellationToken cancellationToken)
        {
            this.state = state;
            Connection = connection;
            Command = command;
            Scripts = scripts;
            ExecuteException = executeException;
            RecordsAffected = recordsAffected;
#if !NETSTANDARD2_0
            Batch = batch;
            BatchCommand = batchCommand;
#endif
            ExecutionTime = executionTime;
            CancellationToken = cancellationToken;
            StackTrace = stackTrace;
            FullTime = fullTime;
            Argss = argss;
            Args = args;
            Transaction = transaction;
        }
        internal readonly ScriptExecutState state;

        public ScriptExecutState State => state;

        public DbConnection Connection { get; }

        public DbCommand? Command { get; }

        public string? Script => Command?.CommandText
#if !NETSTANDARD2_0
            ?? BatchCommand?.CommandText
#endif
            ;

        public IEnumerable<string>? Scripts { get; }

        public IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? Argss { get; }

        public IEnumerable<KeyValuePair<string, object?>>? Args { get; }

        public Exception? ExecuteException { get; }

        public int? RecordsAffected { get; }

#if !NETSTANDARD2_0
        public DbBatch? Batch { get; }

        public DbBatchCommand? BatchCommand { get; }
#endif
        public TimeSpan? ExecutionTime { get; }

        public TimeSpan? FullTime { get; }

        public StackTrace? StackTrace { get; }

        public DbTransaction? Transaction { get; }

        public CancellationToken CancellationToken { get; }

        public IQueryTranslateResult? TranslateResult
        {
            get
            {
                if (Command != null)
                {
                    return QueryTranslateResult.Create(Command.CommandText, Args);
                }
                return null;
            }
        }
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
            if (state== ScriptExecutState.EndReading)
            {
                s.Append("Read ");
            }
            else
            {
                s.Append("Executed ");
            }
            s.Append('(');
            if (ExecutionTime == null)
            {
                s.Append('0');
            }
            else
            {
                s.Append(ExecutionTime.Value.TotalMilliseconds.ToString("F2"));
            }
            s.Append("ms)T(");
            if (FullTime == null)
            {
                s.Append('0');
            }
            else
            {
                s.Append(FullTime.Value.TotalMilliseconds.ToString("F2"));
            }
            s.Append("ms) [");
            if (Args != null)
            {
                s.Append("Pars=");
                s.Append(string.Join(",", Args.Select(x => $"[{x.Key}='{x.Value}'({GetTypeName(x.Value)})]")));
            }
            if (Command != null)
            {
                s.Append(", Timeout=");
                s.Append(Command.CommandTimeout);
            }
            s.AppendLine("]");
            s.AppendLine(Script);
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
            if (state == ScriptExecutState.Executed||state== ScriptExecutState.EndReading)
            {
                return ToExecuteString();
            }
            else if (state== ScriptExecutState.Exception)
            {
                return ToExceptionString();
            }
            return null;
        }
        public static ScriptExecuteEventArgs Begin(DbConnection connection, IEnumerable<string>? scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? args, StackTrace? stackTrace, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Begin,
                connection,
                null,
                scripts,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif                
                null,
                null,
                stackTrace,
                args,
                null,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs CreatedCommand(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.CreatedCommand,
                connection,
                command,
                scripts,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                null,
                null,
                stackTrace,
                args == null ? null : Enumerable.Repeat(args, 1),
                args,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs LoadCommand(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.LoaedCommand,
                connection,
                command,
                scripts,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                null,
                null,
                stackTrace,
                args == null ? null : Enumerable.Repeat(args, 1),
                args,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs Executed(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, IEnumerable<KeyValuePair<string, object?>>? args, int recordsAffected, TimeSpan executionTime, TimeSpan fullTime, StackTrace? stackTrace, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Executed,
                connection,
                command,
                scripts,
                null,
                recordsAffected,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                executionTime,
                fullTime,
                stackTrace,
                args == null ? null : Enumerable.Repeat(args, 1),
                args,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs Exception(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, IEnumerable<KeyValuePair<string, object?>>? args, Exception exception, TimeSpan? executionTime, TimeSpan fullTime, StackTrace? stackTrace, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Exception,
                connection,
                command,
                scripts,
                exception,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                executionTime,
                fullTime,
                stackTrace,
                args == null ? null : Enumerable.Repeat(args, 1),
                args,
                dbTransaction,
                token);
        }
#if !NETSTANDARD2_0
        public static ScriptExecuteEventArgs CreatedBatch(DbConnection connection, IEnumerable<string>? scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? args, DbBatch batch, StackTrace? stackTrace, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.CreatedBatch,
                connection,
                null,
                scripts,
                null,
                null,
                batch,
                null,
                null,
                null,
                stackTrace,
                args,
                null,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs LoadBatchItem(DbConnection connection, IEnumerable<string>? scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? args, DbBatch batch, DbBatchCommand command, StackTrace? stackTrace, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.LoadBatchItem,
                connection,
                null,
                scripts,
                null,
                null,
                batch,
                command,
                null,
                null,
                stackTrace,
                args,
                null,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs ExecutedBatch(DbConnection connection, IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? args, DbBatch batch, TimeSpan? executionTime, TimeSpan fullTime, StackTrace? stackTrace, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.ExecutedBatch,
                connection,
                null,
                scripts,
                null,
                null,
                batch,
                null,
                executionTime,
                fullTime,
                stackTrace,
                args,
                null,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs BatchException(DbConnection connection, IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? args, DbBatch batch, Exception exception, TimeSpan? executionTime, TimeSpan fullTime, StackTrace? stackTrace, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.BatchException,
                connection,
                null,
                scripts,
                exception,
                null,
                batch,
                null,
                executionTime,
                fullTime,
                stackTrace,
                args,
                null,
                dbTransaction,
                token);
        }
#endif
        public static ScriptExecuteEventArgs Skip(DbConnection connection, IEnumerable<string> scripts, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.Skip,
                connection,
                null,
                scripts,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                null,
                null,
                stackTrace,
                args == null ? null : Enumerable.Repeat(args, 1),
                args,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs StartReading(DbConnection connection, DbCommand command, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, TimeSpan? executingTime, TimeSpan? fullTime, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.StartReading,
                connection,
                command,
                new[] { command.CommandText },
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                executingTime,
                fullTime,
                stackTrace,
                args == null ? null : Enumerable.Repeat(args, 1),
                args,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs EndReading(DbConnection connection, DbCommand command, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, TimeSpan? executingTime, TimeSpan? fullTime, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.EndReading,
                connection,
                command,
                new[] { command.CommandText },
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                executingTime,
                fullTime,
                stackTrace,
                args == null ? null : Enumerable.Repeat(args, 1),
                args,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs BeginTranscation(DbConnection connection, StackTrace? stackTrace, TimeSpan? executingTime, TimeSpan? fullTime, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.BeginTransaction,
                connection,
                null,
                null,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                executingTime,
                fullTime,
                stackTrace,
                null,
                null,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs CommitedTransaction(DbConnection connection, StackTrace? stackTrace, TimeSpan? executingTime, TimeSpan? fullTime, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.CommitedTransaction,
                connection,
                null,
                null,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                executingTime,
                fullTime,
                stackTrace,
                null,
                null,
                dbTransaction,
                token);
        }
        public static ScriptExecuteEventArgs RollbackedTransaction(DbConnection connection, StackTrace? stackTrace, TimeSpan? executingTime, TimeSpan? fullTime, DbTransaction? dbTransaction, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.RollbackedTransaction,
                connection,
                null,
                null,
                null,
                null,
#if !NETSTANDARD2_0
                null,
                null,
#endif
                executingTime,
                fullTime,
                stackTrace,
                null,
                null,
                dbTransaction,
                token);
        }
    }
}
