using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public class DefaultScriptExecuter : IDbScriptExecuter, IDbStackTraceScriptExecuter
    {
        private static readonly IReadOnlyList<MethodBase> Methods = typeof(DefaultScriptExecuter).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(x => !x.IsSpecialName && x.DeclaringType == typeof(DefaultScriptExecuter))
            .ToArray();

        private static readonly double TickFrequency = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;

        private static readonly Assembly Assembly = typeof(DefaultScriptExecuter).Assembly;

        public static StackFrame? GetSourceFrame(StackTrace stackTrace)
        {
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                if (frame != null && frame.HasSource() && frame.HasMethod())
                {
                    var method = frame.GetMethod();
                    if (method != null &&
                        method.DeclaringType != null &&
                        method.DeclaringType.Assembly != Assembly)
                    {
                        return frame;
                    }
                }
            }
            return null;
        }

        protected static TimeSpan GetElapsedTime(long startingTimestamp)
        {
            return new TimeSpan((long)((Stopwatch.GetTimestamp() - startingTimestamp) * TickFrequency));
        }
        public static bool IsExecuterMethod(MethodBase method)
        {
            return Methods.Contains(method);
        }

        public const int DefaultCommandTimeout = 60;

        public DefaultScriptExecuter(DbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public DbConnection Connection { get; }

        public int CommandTimeout { get; set; }

        public bool UseBatch { get; set; }

        public bool CaptureStackTrace { get; set; }

        public bool StackTraceNeedFileInfo { get; set; } = true;

        public event EventHandler<ScriptExecuteEventArgs>? ScriptStated;

        public Task<int> ExecuteAsync(string script, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default)
        {
            return ExecuteAsync(script, null, args, GetStackTrace(), token);
        }

        private StackTrace? GetStackTrace()
        {
            if (CaptureStackTrace)
            {
                return new StackTrace(StackTraceNeedFileInfo);
            }
            return null;
        }
        private static DbType GetDbType(object? input)
        {
            if (input == null)
            {
                return DbType.String;
            }

            var typeCode = Type.GetTypeCode(input.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.SByte:
                    return DbType.SByte;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.UInt16:
                    return DbType.UInt16;
                case TypeCode.UInt32:
                    return DbType.UInt32;
                case TypeCode.UInt64:
                    return DbType.UInt64;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                case TypeCode.String:
                case TypeCode.Empty:
                default:
                    return DbType.String;
            }
        }

        protected virtual void LoadParamters(DbCommand command, IEnumerable<KeyValuePair<string, object?>>? args = null) 
        {
            if (args != null)
            {
                foreach (var item in args)
                {
                    if (item.Value is DbParameter dbp)
                    {
                        command.Parameters.Add(dbp);
                    }
                    else
                    {
                        var par = command.CreateParameter();
                        par.ParameterName = item.Key;
                        par.DbType = GetDbType(item.Value);
                        command.Parameters.Add(par);
                    }
                }
            }
        }
        protected virtual async Task<int> ExecuteAsync(string script, IEnumerable<string>? scripts,IEnumerable<KeyValuePair<string,object?>>? args, StackTrace? stackTrace, CancellationToken token = default)
        {
            if (IsEmptyScript(script))
            {
                ScriptStated?.Invoke(this, ScriptExecuteEventArgs.Skip(Connection, new[] { script }, args, stackTrace, token));
                return 0;
            }
            var fullStartTime = Stopwatch.GetTimestamp();
            using (var command = Connection.CreateCommand())
            {
                LoadParamters(command, args);
                try
                {
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CreatedCommand(Connection, command, scripts,args, stackTrace, token));
                    command.CommandText = script;
                    command.CommandTimeout = CommandTimeout;
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.LoadCommand(Connection, command, scripts, args, stackTrace, token));
                    var startTime = Stopwatch.GetTimestamp();
                    var res = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.Executed(Connection, command, scripts, args, res, GetElapsedTime(startTime), GetElapsedTime(fullStartTime), stackTrace, token));
                    return res;
                }
                catch (Exception ex)
                {
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.Exception(Connection, command, scripts, args, ex, null, GetElapsedTime(fullStartTime), stackTrace, token));
                    throw;
                }
            }
        }
        protected virtual bool IsEmptyScript(string script)
        {
            return string.IsNullOrWhiteSpace(script) || AllLineStartWith(script, "--");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool AllLineStartWith(string script, string startWith)
        {
            var startIndex = 0;
            var length = script.Length;
            for (int i = 0; i < length; i++)
            {
                if (script[i] == '\n')
                {
                    if (!script.AsSpan(startIndex, i - 1 - startIndex).TrimStart().StartsWith("--".AsSpan()))
                    {
                        return false;
                    }
                    startIndex = i - 1;
                }
            }
            if (startIndex != length)
            {
                return script.AsSpan(startIndex).TrimStart().StartsWith("--".AsSpan());
            }
            return true;
        }
        private async Task<int> ExecuteBatchSlowAsync(IEnumerable<string> scripts,  StackTrace? stackTrace, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null, CancellationToken token = default)
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
                    res += await ExecuteAsync(item, scripts, args, stackTrace, token).ConfigureAwait(false);
                }
            }
            finally
            {
                argEnumerator?.Dispose();
            }
            return res;
        }
#if !NETSTANDARD2_0
        protected async Task<int?> BatchExecuteAdoAsync(IEnumerable<string> scripts, StackTrace? stackTrace, IEnumerable<IEnumerable<KeyValuePair<string,object?>>>? argss = null, CancellationToken token = default)
        {
            if (UseBatch && Connection.CanCreateBatch)
            {
                var fullStartTime = Stopwatch.GetTimestamp();
                using (var batch = Connection.CreateBatch())
                {
                    var enu = argss?.GetEnumerator();
                    try
                    {
                        ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CreatedBatch(Connection, scripts,argss, batch, stackTrace, token));
                        foreach (var item in scripts)
                        {
                            IEnumerable<KeyValuePair<string, object?>>? args = null;
                            if (enu!=null&&enu.MoveNext())
                            {
                                args = enu.Current;
                            }
                            if (IsEmptyScript(item))
                            {
                                ScriptStated?.Invoke(this, ScriptExecuteEventArgs.Skip(Connection, new[] { item }, args, stackTrace, token));
                                continue;
                            }
                            var comm = batch.CreateBatchCommand();
                            comm.CommandText = item;
                            if (args != null)
                            {
                                foreach (var arg in args)
                                {
                                    //TODO: par load
                                    comm.Parameters.Add(arg.Value);
                                }
                            }
                            ScriptStated?.Invoke(this, ScriptExecuteEventArgs.LoadBatchItem(Connection, scripts, argss,batch, comm, stackTrace, token));
                        }
                        batch.Timeout = CommandTimeout;
                        var startTime = Stopwatch.GetTimestamp();
                        var res = await batch.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                        ScriptStated?.Invoke(this, ScriptExecuteEventArgs.ExecutedBatch(Connection, scripts,argss, batch, GetElapsedTime(startTime), GetElapsedTime(fullStartTime), stackTrace, token));
                        return res;
                    }
                    catch (Exception ex)
                    {
                        ScriptStated?.Invoke(this, ScriptExecuteEventArgs.BatchException(Connection, scripts, argss, batch, ex, null, GetElapsedTime(fullStartTime), stackTrace, token));
                        throw;
                    }
                    finally
                    {
                        enu?.Dispose();
                    }
                }
            }
            return null;
        }
#endif
        public Task<int> ExecuteBatchAsync(IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null, CancellationToken token = default)
        {
            return ExecuteBatchAsync(scripts, GetStackTrace(),argss, token);
        }

        public async Task ReadAsync(string script, ReadDataHandler handler, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default)
        {
            var stackTrace = GetStackTrace();
            var fullStartTime = Stopwatch.GetTimestamp();
            using (var command = Connection.CreateCommand())
            {
                try
                {
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CreatedCommand(Connection, command, null,args, stackTrace, token));
                    command.CommandText = script;
                    command.CommandTimeout = CommandTimeout;
                    LoadParamters(command, args);
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.LoadCommand(Connection, command, null, args, stackTrace, token));
                    var startTime = Stopwatch.GetTimestamp();
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.StartReading(Connection, command,args, stackTrace, GetElapsedTime(startTime), GetElapsedTime(fullStartTime), token));
                    using (var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false))
                    {
                        await handler(this, new ReadingDataArgs(script, reader, token));
                    }
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.EndReading(Connection, command, args, stackTrace, GetElapsedTime(startTime), GetElapsedTime(fullStartTime), token));
                }
                catch (Exception ex)
                {
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.Exception(Connection, command, null,args, ex, null, GetElapsedTime(fullStartTime), stackTrace, token));
                    throw;
                }
            }
        }

        public Task<int> ExecuteAsync(string script, StackTrace? stackTrace, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default)
        {
            return ExecuteAsync(script, null, args, stackTrace, token);
        }

        public async Task<int> ExecuteBatchAsync(IEnumerable<string> scripts, StackTrace? stackTrace, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss = null, CancellationToken token = default)
        {
            if (!scripts.Any())
            {
                return 0;
            }
            stackTrace ??= GetStackTrace();
#if !NETSTANDARD2_0
            var res = await BatchExecuteAdoAsync(scripts, stackTrace, argss, token).ConfigureAwait(false);
            if (res != null)
            {
                return res.Value;
            }
#endif
            return await ExecuteBatchSlowAsync(scripts, stackTrace, argss, token).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }

    }
}
