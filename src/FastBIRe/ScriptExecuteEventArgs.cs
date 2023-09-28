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
            StackTrace? stackTrace,
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

        public Exception? ExecuteException { get; }

        public int? RecordsAffected { get; }

#if !NETSTANDARD2_0
        public DbBatch? Batch { get; }

        public DbBatchCommand? BatchCommand { get; }
#endif
        public TimeSpan? ExecutionTime { get; }

        public StackTrace? StackTrace { get; }

        public CancellationToken CancellationToken { get; }

        public static ScriptExecuteEventArgs Begin(DbConnection connection, IEnumerable<string>? scripts,StackTrace? stackTrace, CancellationToken token)
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
                stackTrace,
                token);
        }
        public static ScriptExecuteEventArgs CreatedCommand(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, StackTrace? stackTrace, CancellationToken token)
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
                stackTrace,
                token);
        }
        public static ScriptExecuteEventArgs LoaedCommand(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, StackTrace? stackTrace, CancellationToken token)
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
                stackTrace,
                token);
        }
        public static ScriptExecuteEventArgs Executed(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, int recordsAffected,TimeSpan executionTime, StackTrace? stackTrace, CancellationToken token)
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
                stackTrace,
                token);
        }
        public static ScriptExecuteEventArgs Exception(DbConnection connection, DbCommand command, IEnumerable<string>? scripts, Exception exception, TimeSpan executionTime, StackTrace? stackTrace, CancellationToken token)
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
                stackTrace,
                token);
        }
#if !NETSTANDARD2_0
        public static ScriptExecuteEventArgs CreatedBatch(DbConnection connection, IEnumerable<string>? scripts, DbBatch batch, StackTrace? stackTrace, CancellationToken token)
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
                stackTrace,
                token);
        }
        public static ScriptExecuteEventArgs LoadBatchItem(DbConnection connection, IEnumerable<string>? scripts, DbBatch batch, DbBatchCommand command, StackTrace? stackTrace, CancellationToken token)
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
                stackTrace,
                token);
        }
        public static ScriptExecuteEventArgs ExecutedBatch(DbConnection connection, IEnumerable<string> scripts, DbBatch batch, TimeSpan executionTime, StackTrace? stackTrace, CancellationToken token)
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
                stackTrace,
                token);
        }
        public static ScriptExecuteEventArgs BatchException(DbConnection connection, IEnumerable<string> scripts, DbBatch batch,Exception exception, TimeSpan executionTime, StackTrace? stackTrace, CancellationToken token)
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
                stackTrace,
                token);
        }
#endif
    }
}
