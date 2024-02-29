using FastBIRe.Internals;
using System.Data.Common;
using System.Diagnostics;

namespace FastBIRe
{
    public partial class DefaultScriptExecuter
    {
        public Task<int> ExecuteAsync(string script, IEnumerable<KeyValuePair<string, object?>>? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return ExecuteAsync(script, null, args, GetStackTrace(),transaction, token);
        }

        protected virtual async Task<int> ExecuteAsync(string script, IEnumerable<string>? scripts, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace, DbTransaction? transaction = null, CancellationToken token = default)
        {
            var s = new CommandState(this, stackTrace, new ScriptUnit(script, args));
            s.RaiseBegin();
            if (s.ScriptIsEmpty)
            {
                s.EndTime();
                s.RaiseSkip();
                return 0;
            }
            using (var command = Connection.CreateCommand())
            {
                try
                {
                    command.Transaction = dbTransaction ?? transaction;

                    s.RaiseCreatedCommand(command);
                    s.LoadParamters(command);
                    s.RaiseLoadCommand(command);
                    s.ExecuteStartTime = Stopwatch.GetTimestamp();
                    var res = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                    s.EndTime();
                    s.RaiseExecuted(command, res);
                    return res;
                }
                catch (Exception ex)
                {
                    s.RaiseException(command
#if !NETSTANDARD2_0
                       , null
#endif
                        , ex);
                    throw;
                }
            }
        }
        private async Task<int> ExecuteBatchSlowAsync(IEnumerable<string> scripts, StackTrace? stackTrace, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            var res = 0;
            var argEnumerator = argss?.GetEnumerator();
            try
            {
                IEnumerable<KeyValuePair<string, object?>>? args = null;
                if (argss != null && argEnumerator!.MoveNext())
                {
                    args = argEnumerator.Current;
                }
                foreach (var item in scripts)
                {
                    res += await ExecuteAsync(item, scripts, args, stackTrace,transaction, token);
                }
            }
            finally
            {
                argEnumerator?.Dispose();
            }
            return res;
        }
#if !NETSTANDARD2_0
        protected async Task<int?> BatchExecuteAdoAsync(IEnumerable<string> scripts, StackTrace? stackTrace, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            var s = new CommandState(this, stackTrace, CreateScriptUnits(scripts, argss));
            s.RaiseBegin();
            if (UseBatch && Connection.CanCreateBatch)
            {
                s.ExecuteStartTime = Stopwatch.GetTimestamp();
                using (var batch = Connection.CreateBatch())
                {
                    try
                    {
                        batch.Transaction = dbTransaction??transaction;
                        s.RaiseCreateBatch(batch);
                        foreach (var item in s.ScriptUnits!)
                        {
                            if (item.IsEmptyScript)
                            {
                                s.RaiseSkip(item);
                                continue;
                            }
                            var comm = batch.CreateBatchCommand();
                            s.LoadParamters(batch, comm, item);
                            s.RaiseLoadBatchItem(batch, comm);
                        }
                        batch.Timeout = CommandTimeout;
                        var startTime = Stopwatch.GetTimestamp();
                        var res = await batch.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                        s.EndTime();
                        s.RaiseExecutedBatch(batch, res);
                        return res;
                    }
                    catch (Exception ex)
                    {
                        s.RaiseException(null, batch, ex);
                        throw;
                    }
                }
            }
            return null;
        }
#endif
        public Task<int> ExecuteBatchAsync(IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return ExecuteBatchAsync(scripts, GetStackTrace(), argss,transaction, token);
        }
        public async Task<TResult> ReadResultAsync<TResult>(string script, ReadDataResultHandler<TResult> handler, IEnumerable<KeyValuePair<string, object?>>? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            var s = new CommandState(this, GetStackTrace(), new ScriptUnit(script, args));
            s.RaiseBegin();
            using (var command = Connection.CreateCommand())
            {
                try
                {
                    command.Transaction = dbTransaction ?? transaction;

                    s.RaiseCreatedCommand(command);
                    s.LoadParamters(command);
                    s.RaiseLoadCommand(command);
                    s.ExecuteStartTime = Stopwatch.GetTimestamp();
                    s.RaiseStartReading(command);
                    TResult result;
                    using (var reader = await command.ExecuteReaderAsync(token))
                    {
                        result = await handler(this, new ReadingDataArgs(script, reader, QueryTranslateResult.Create(script, args), token));
                    }
                    s.EndTime();
                    s.RaiseEndReading(command);
                    return result;
                }
                catch (Exception ex)
                {
                    s.RaiseException(command
#if !NETSTANDARD2_0
                        , null
#endif
                        , ex);
                    throw;
                }
            }
        }
        public Task ReadAsync(string script, ReadDataHandler handler, IEnumerable<KeyValuePair<string, object?>>? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return ReadResultAsync(script, async (o, e) =>
            {
                await handler(o, e);
                return true;
            }, args,transaction, token);
        }
        public async Task<IScriptReadResult> ReadAsync(string script, IEnumerable<KeyValuePair<string, object?>>? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            var s = new CommandState(this, GetStackTrace(), new ScriptUnit(script, args));
            s.RaiseBegin();
            var command = Connection.CreateCommand();
            try
            {
                command.Transaction = dbTransaction ?? transaction;
                s.RaiseCreatedCommand(command);
                s.LoadParamters(command);
                s.RaiseLoadCommand(command);
                s.RaiseStartReading(command);
                s.ExecuteStartTime = Stopwatch.GetTimestamp();
                var reader = await command.ExecuteReaderAsync(token);
                var readArg = new ReadingDataArgs(script, reader, QueryTranslateResult.Create(script, args), token);
                return new DefaultScriptReadResult(this, readArg, command, () =>
                {
                    s.EndTime();
                    s.RaiseEndReading(command);
                });
            }
            catch (Exception ex)
            {
                s.RaiseException(command
#if !NETSTANDARD2_0
                    , null
#endif
                    , ex);
                throw;
            }
        }
        public Task<int> ExecuteAsync(string script, StackTrace? stackTrace, IEnumerable<KeyValuePair<string, object?>>? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return ExecuteAsync(script, null, args, stackTrace, transaction, token);
        }

        public async Task<int> ExecuteBatchAsync(IEnumerable<string> scripts, StackTrace? stackTrace, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            if (!scripts.Any())
            {
                return 0;
            }
            stackTrace ??= GetStackTrace();
#if !NETSTANDARD2_0
            var res = await BatchExecuteAdoAsync(scripts, stackTrace, argss,transaction, token);
            if (res != null)
            {
                return res.Value;
            }
#endif
            return await ExecuteBatchSlowAsync(scripts, stackTrace, argss, transaction, token);
        }

    }
}
