using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

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
