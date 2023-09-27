using System.Data.Common;
using System.Diagnostics;

namespace FastBIRe
{
    public class DefaultScriptExecuter : IScriptExecuter
    {
        public const int DefaultCommandTimeout = 60;

        public DefaultScriptExecuter(DbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public DbConnection Connection { get; }

        public int CommandTimeout { get; set; }

        public bool UseBatch { get; set; }

        public event EventHandler<ScriptExecuteEventArgs>? ScriptStated;

        public Task<int> ExecuteAsync(string script, CancellationToken token)
        {
            return ExecuteAsync(script, null, token);
        }
        private static readonly double TickFrequency = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;

        protected static TimeSpan GetElapsedTime(long startingTimestamp)
        {
            return new TimeSpan((long)((Stopwatch.GetTimestamp() - startingTimestamp) * TickFrequency));
        }
        protected virtual async Task<int> ExecuteAsync(string script, IEnumerable<string>? scripts, CancellationToken token)
        {
            using (var command = Connection.CreateCommand())
            {
                ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CreatedCommand(Connection, command, scripts, token));
                command.CommandText = script;
                command.CommandTimeout = CommandTimeout;
                ScriptStated?.Invoke(this, ScriptExecuteEventArgs.LoaedCommand(Connection, command, scripts, token));
                var startTime = Stopwatch.GetTimestamp();
                try
                {
                    var res = await command.ExecuteNonQueryAsync(token);
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.Executed(Connection, command, scripts, res, GetElapsedTime(startTime), token));
                    return res;
                }
                catch (Exception ex)
                {
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.Exception(Connection, command, scripts, ex, GetElapsedTime(startTime), token));
                    throw;
                }
            }
        }
        private async Task<int> ExecuteBatchAsync(IEnumerable<string> scripts, CancellationToken token)
        {
            var res = 0;
            foreach (var item in scripts)
            {
                res += await ExecuteAsync(item, token);
            }
            return res;
        }
        public async Task<int> ExecuteAsync(IEnumerable<string> scripts, CancellationToken token)
        {
#if !NETSTANDARD2_0
            if (UseBatch && Connection.CanCreateBatch)
            {
                using (var batch = Connection.CreateBatch())
                {
                    ScriptStated?.Invoke(this, ScriptExecuteEventArgs.CreatedBatch(Connection, scripts, batch, token));

                    foreach (var item in scripts)
                    {
                        var comm = batch.CreateBatchCommand();
                        comm.CommandText = item;
                        ScriptStated?.Invoke(this, ScriptExecuteEventArgs.LoadBatchItem(Connection, scripts, batch, comm, token));
                    }
                    batch.Timeout = CommandTimeout;
                    var startTime = Stopwatch.GetTimestamp();
                    try
                    {
                        var res = await batch.ExecuteNonQueryAsync(token);
                        ScriptStated?.Invoke(this, ScriptExecuteEventArgs.ExecutedBatch(Connection, scripts, batch, GetElapsedTime(startTime), token));
                        return res;
                    }
                    catch (Exception ex)
                    {
                        ScriptStated?.Invoke(this, ScriptExecuteEventArgs.BatchException(Connection, scripts, batch, ex, GetElapsedTime(startTime), token));
                        throw;
                    }
                }
            }
#endif
            return await ExecuteBatchAsync(scripts, token);
        }
    }
}
