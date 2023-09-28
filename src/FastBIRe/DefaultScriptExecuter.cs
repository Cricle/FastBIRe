using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public class DefaultScriptExecuter : IScriptExecuter
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
                        method.DeclaringType!=null&&
                        method.DeclaringType.Assembly!= Assembly)
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

        public Task<int> ExecuteAsync(string script, CancellationToken token)
        {
            StackTrace? stackTrace = GetStackTrace();
            return ExecuteAsync(script, null, stackTrace, token);
        }

        private StackTrace? GetStackTrace() 
        {
            if (CaptureStackTrace)
            {
                return new StackTrace(StackTraceNeedFileInfo);
            }
            return null;
        }
        protected virtual async Task<int> ExecuteAsync(string script, IEnumerable<string>? scripts,StackTrace? stackTrace, CancellationToken token)
        {
            using (var command = Connection.CreateCommand())
            {
                ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CreatedCommand(Connection, command, scripts, stackTrace, token));
                command.CommandText = script;
                command.CommandTimeout = CommandTimeout;
                ScriptStated?.Invoke(this, ScriptExecuteEventArgs.LoaedCommand(Connection, command, scripts, stackTrace, token));
                var startTime = Stopwatch.GetTimestamp();
                try
                {
                    var res = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.Executed(Connection, command, scripts, res, GetElapsedTime(startTime), stackTrace, token));
                    return res;
                }
                catch (Exception ex)
                {
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.Exception(Connection, command, scripts, ex, GetElapsedTime(startTime), stackTrace, token));
                    throw;
                }
            }
        }
        private async Task<int> ExecuteBatchAsync(IEnumerable<string> scripts,StackTrace? stackTrace, CancellationToken token)
        {
            var res = 0;
            foreach (var item in scripts)
            {
                res += await ExecuteAsync(item,scripts, stackTrace, token).ConfigureAwait(false);
            }
            return res;
        }
        public async Task<int> ExecuteAsync(IEnumerable<string> scripts, CancellationToken token)
        {
            StackTrace? stackTrace = GetStackTrace();
#if !NETSTANDARD2_0
            if (UseBatch && Connection.CanCreateBatch)
            {
                using (var batch = Connection.CreateBatch())
                {
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CreatedBatch(Connection, scripts, batch, stackTrace, token));

                    foreach (var item in scripts)
                    {
                        var comm = batch.CreateBatchCommand();
                        comm.CommandText = item;
                        ScriptStated?.Invoke(this, ScriptExecuteEventArgs.LoadBatchItem(Connection, scripts, batch, comm, stackTrace, token));
                    }
                    batch.Timeout = CommandTimeout;
                    var startTime = Stopwatch.GetTimestamp();
                    try
                    {
                        var res = await batch.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                        ScriptStated?.Invoke(this, ScriptExecuteEventArgs.ExecutedBatch(Connection, scripts, batch, GetElapsedTime(startTime), stackTrace, token));
                        return res;
                    }
                    catch (Exception ex)
                    {
                        ScriptStated?.Invoke(this, ScriptExecuteEventArgs.BatchException(Connection, scripts, batch, ex, GetElapsedTime(startTime), stackTrace, token));
                        throw;
                    }
                }
            }
#endif
            return await ExecuteBatchAsync(scripts,stackTrace, token).ConfigureAwait(false);
        }
    }
}
