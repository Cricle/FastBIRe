using System.Diagnostics;

namespace FastBIRe
{
    public partial class DefaultScriptExecuter
    {
        public int Execute(string script, IEnumerable<KeyValuePair<string, object?>>? args = null)
        {
            return Execute(script, null, args, GetStackTrace());
        }

        public int ExecuteBatch(IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null)
        {
            return ExecuteBatch(scripts, GetStackTrace(), argss);
        }

        public void Read(string script, ReadDataHandlerSync handler, IEnumerable<KeyValuePair<string, object?>>? args = null)
        {
            ReadResult<bool>(script, (o, e) =>
            {
                handler(o, e);
                return false;
            }, args);
        }

        public IScriptReadResult Read(string script, IEnumerable<KeyValuePair<string, object?>>? args = null)
        {
            var s = new CommandState(this, GetStackTrace(), new ScriptUnit(script, args));
            s.RaiseBegin();
            var command = Connection.CreateCommand();
            try
            {
                s.RaiseCreatedCommand(command);
                s.LoadParamters(command);
                s.RaiseLoadCommand(command);
                s.RaiseStartReading(command);
                s.ExecuteStartTime = Stopwatch.GetTimestamp();
                var reader = command.ExecuteReader();
                var readArg = new ReadingDataArgs(script, reader, QueryTranslateResult.Create(script, args), CancellationToken.None);
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
        public int ExecuteBatch(IEnumerable<string> scripts, StackTrace? stackTrace, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null)
        {
            if (!scripts.Any())
            {
                return 0;
            }
            stackTrace ??= GetStackTrace();
#if !NETSTANDARD2_0
            var res = BatchExecuteAdo(scripts, stackTrace, argss);
            if (res != null)
            {
                return res.Value;
            }
#endif
            return ExecuteBatchSlow(scripts, stackTrace, argss);
        }

        public TResult ReadResult<TResult>(string script, ReadDataResultHandlerSync<TResult> handler, IEnumerable<KeyValuePair<string, object?>>? args = null)
        {
            var s = new CommandState(this, GetStackTrace(), new ScriptUnit(script, args));
            s.RaiseBegin();
            using (var command = Connection.CreateCommand())
            {
                try
                {
                    s.RaiseCreatedCommand(command);
                    s.LoadParamters(command);
                    s.RaiseLoadCommand(command);
                    s.ExecuteStartTime = Stopwatch.GetTimestamp();
                    s.RaiseStartReading(command);
                    TResult result;
                    using (var reader = command.ExecuteReader())
                    {
                        result = handler(this, new ReadingDataArgs(script, reader, QueryTranslateResult.Create(script, args), default));
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
        private int ExecuteBatchSlow(IEnumerable<string> scripts, StackTrace? stackTrace, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null)
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
                    res += Execute(item, scripts, args, stackTrace);
                }
            }
            finally
            {
                argEnumerator?.Dispose();
            }
            return res;
        }
#if !NETSTANDARD2_0
        protected int? BatchExecuteAdo(IEnumerable<string> scripts, StackTrace? stackTrace, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null)
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
                        batch.Transaction = dbTransaction;
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
                        var res = batch.ExecuteNonQuery();
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

        protected virtual int Execute(string script, IEnumerable<string>? scripts, IEnumerable<KeyValuePair<string, object?>>? args, StackTrace? stackTrace)
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
                    s.RaiseCreatedCommand(command);
                    s.LoadParamters(command);
                    s.RaiseLoadCommand(command);
                    s.ExecuteStartTime = Stopwatch.GetTimestamp();
                    var res = command.ExecuteNonQuery();
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
    }
}
