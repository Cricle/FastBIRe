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
            if (dbTransaction != null)
            {
                throw new InvalidOperationException("The transaction is not running");
            }
        }

        public void BeginTransaction(IsolationLevel level = IsolationLevel.Unspecified)
        {
            ThrowIfTransactionNotNull();
            dbTransaction = Connection.BeginTransaction(level);
        }

        public
#if NET6_0_OR_GREATER
            async
#endif
            Task BeginTransactionAsync(IsolationLevel level = IsolationLevel.Unspecified, CancellationToken token = default)
        {
            var fullStartTime = Stopwatch.GetTimestamp();
            ThrowIfTransactionNotNull();
#if NET6_0_OR_GREATER
            dbTransaction = await Connection.BeginTransactionAsync(level, token);
#else
            dbTransaction = Connection.BeginTransaction(level);
#endif
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.BeginTranscation(Connection, GetStackTrace(), GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), dbTransaction, token));
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
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CommitedTranscation(Connection, GetStackTrace(), GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), dbTransaction, default));
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
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CommitedTranscation(Connection, GetStackTrace(), GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), dbTransaction, token));
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
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.RollbackedTranscation(Connection, GetStackTrace(), GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), dbTransaction, default));
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
            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.RollbackedTranscation(Connection, GetStackTrace(), GetElapsedTime(fullStartTime), GetElapsedTime(fullStartTime), dbTransaction, token));
#if !NET6_0_OR_GREATER
            return Task.CompletedTask;
#endif
        }
    }
}
