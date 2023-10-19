using System.Data.Common;
using System.Diagnostics;

namespace FastBIRe
{
    public readonly struct ScriptExecuteEventArgs
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
            CancellationToken cancellationToken)
        {
            State = state;
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
        }

        public ScriptExecutState State { get; }

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

        public CancellationToken CancellationToken { get; }

        public static ScriptExecuteEventArgs Begin(DbConnection connection, IEnumerable<string>? scripts,IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? args, StackTrace? stackTrace, CancellationToken token)
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
                token);
        }
        public static ScriptExecuteEventArgs CreatedCommand(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, CancellationToken token)
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
                null,
                args,
                token);
        }
        public static ScriptExecuteEventArgs LoadCommand(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, CancellationToken token)
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
                null,
                args,
                token);
        }
        public static ScriptExecuteEventArgs Executed(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, IEnumerable<KeyValuePair<string, object?>>? args, int recordsAffected, TimeSpan executionTime, TimeSpan fullTime, StackTrace? stackTrace, CancellationToken token)
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
                null,
                args,
                token);
        }
        public static ScriptExecuteEventArgs Exception(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, IEnumerable<KeyValuePair<string, object?>>? args, Exception exception, TimeSpan? executionTime, TimeSpan fullTime, StackTrace? stackTrace, CancellationToken token)
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
                null,
                args,
                token);
        }
#if !NETSTANDARD2_0
        public static ScriptExecuteEventArgs CreatedBatch(DbConnection connection, IEnumerable<string>? scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? args, DbBatch batch, StackTrace? stackTrace, CancellationToken token)
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
                token);
        }
        public static ScriptExecuteEventArgs LoadBatchItem(DbConnection connection, IEnumerable<string>? scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? args, DbBatch batch, DbBatchCommand command, StackTrace? stackTrace, CancellationToken token)
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
                token);
        }
        public static ScriptExecuteEventArgs ExecutedBatch(DbConnection connection, IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? args, DbBatch batch, TimeSpan? executionTime, TimeSpan fullTime, StackTrace? stackTrace, CancellationToken token)
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
                token);
        }
        public static ScriptExecuteEventArgs BatchException(DbConnection connection, IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? args, DbBatch batch, Exception exception, TimeSpan? executionTime, TimeSpan fullTime, StackTrace? stackTrace, CancellationToken token)
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
                token);
        }
#endif
        public static ScriptExecuteEventArgs Skip(DbConnection connection, IEnumerable<string> scripts, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, CancellationToken token)
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
                null,
                args,
                token);
        }
        public static ScriptExecuteEventArgs StartReading(DbConnection connection, DbCommand command, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, TimeSpan? executingTime, TimeSpan? fullTime, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.StartReading,
                connection,
                command,
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
                args,
                token);
        }
        public static ScriptExecuteEventArgs EndReading(DbConnection connection, DbCommand command, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, TimeSpan? executingTime, TimeSpan? fullTime, CancellationToken token)
        {
            return new ScriptExecuteEventArgs(ScriptExecutState.EndReading,
                connection,
                command,
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
                args,
                token);
        }
    }
}
