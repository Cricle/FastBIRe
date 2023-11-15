using BenchmarkDotNet.Attributes;

namespace FastBIRe.Benchmarks.Actions
{
    [MemoryDiagnoser]
    public class ScriptReadBenchmarks : DuckDBBenchmark
    {
        [Params(true, false)]
        public bool CaptureStackTrace { get; set; }

        DefaultScriptExecuter scriptExecuter;
        [GlobalSetup]
        public void Setup()
        {
            scriptExecuter = new DefaultScriptExecuter(Connection);
            scriptExecuter.CaptureStackTrace = CaptureStackTrace;
            scriptExecuter.ScriptStated += ScriptExecuter_ScriptStated;
        }

        private void ScriptExecuter_ScriptStated(object? sender, ScriptExecuteEventArgs e)
        {
        }

        [Benchmark(Baseline = true)]
        public async Task RawAsyncExecute()
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = "SELECT 1";
                using (var reader = await comm.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {

                    }
                }
            }
        }
        [Benchmark]
        public async Task AsyncExecute()
        {
            await scriptExecuter.ReadAsync("SELECT 1", (s, e) =>
            {
                while (e.Reader.Read())
                {

                }
                return Task.CompletedTask;
            });
        }
        [Benchmark]
        public async Task ORMExecute()
        {
            await scriptExecuter.ReadOneAsync<int>("SELECT 1");
        }
    }
}
