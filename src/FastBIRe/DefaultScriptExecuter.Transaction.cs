using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public partial class DefaultScriptExecuter : IDbScriptTransaction
    {
        private DbTransaction? dbTransaction;

        public DbTransaction? Transaction => dbTransaction;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfTransactionNotNull()
        {
            if (dbTransaction != null)
            {
                throw new InvalidOperationException("The transaction is running");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfTransactionNoStart()
        {
            if (dbTransaction == null)
            {
                throw new InvalidOperationException("The transaction is not running");
            }
        }

        public void BeginTransaction(IsolationLevel level = IsolationLevel.Unspecified)
        {
            ThrowIfTransactionNotNull();
            var fullStartTime = Stopwatch.GetTimestamp();
            dbTransaction = Connection.BeginTransaction(level);
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.BeginTranscation(Connection, new TraceUnit(
                GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), GetStackTrace()), dbTransaction));
        }

        public
#if NET6_0_OR_GREATER
            async
#endif
            Task BeginTransactionAsync(IsolationLevel level = IsolationLevel.Unspecified, CancellationToken token = default)
        {
            ThrowIfTransactionNotNull();
            var fullStartTime = Stopwatch.GetTimestamp();
#if NET6_0_OR_GREATER
            dbTransaction = await Connection.BeginTransactionAsync(level, token);
#else
            dbTransaction = Connection.BeginTransaction(level);
#endif
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.BeginTranscation(Connection, new TraceUnit(
                GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), GetStackTrace()), dbTransaction));
#if !NET6_0_OR_GREATER
            return Task.CompletedTask;
#endif
        }

        public void Commit()
        {
            var fullStartTime = Stopwatch.GetTimestamp();
            ThrowIfTransactionNoStart();
            dbTransaction!.Commit();
            dbTransaction = null;
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CommitedTransaction(Connection, new TraceUnit(
                GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), GetStackTrace()), dbTransaction));
        }

        public 
#if NET6_0_OR_GREATER
            async
#endif
            Task CommitAsync(CancellationToken token = default)
        {
            var fullStartTime = Stopwatch.GetTimestamp();
            ThrowIfTransactionNoStart();
#if NET6_0_OR_GREATER
            await dbTransaction!.CommitAsync(token);
#else
            dbTransaction!.Commit();
#endif
            dbTransaction = null;
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CommitedTransaction(Connection, new TraceUnit(
                GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), GetStackTrace()), dbTransaction));
#if !NET6_0_OR_GREATER
            return Task.CompletedTask;
#endif
        }

        public void Rollback()
        {
            var fullStartTime = Stopwatch.GetTimestamp();
            ThrowIfTransactionNoStart();
            dbTransaction!.Rollback();
            dbTransaction = null;
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.RollbackedTransaction(Connection, new TraceUnit(
                GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), GetStackTrace()), dbTransaction));
        }

        public 
#if NET6_0_OR_GREATER
            async
#endif
            Task RollbackAsync(CancellationToken token = default)
        {
            var fullStartTime = Stopwatch.GetTimestamp();
            ThrowIfTransactionNoStart();
#if NET6_0_OR_GREATER
            await dbTransaction!.RollbackAsync(token);
#else
            dbTransaction!.Rollback();
#endif
            dbTransaction = null;
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.RollbackedTransaction(Connection, new TraceUnit(
                GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), GetStackTrace()), dbTransaction));
#if !NET6_0_OR_GREATER
            return Task.CompletedTask;
#endif
        }
    }
}
