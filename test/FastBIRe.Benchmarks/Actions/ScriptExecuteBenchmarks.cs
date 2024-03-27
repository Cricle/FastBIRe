using BenchmarkDotNet.Attributes;

namespace FastBIRe.Benchmarks.Actions
{
    [MemoryDiagnoser]
    public class ScriptExecuteBenchmarks : DuckDBBenchmark
    {
        DefaultScriptExecuter scriptExecuter;

        [Params(true, false)]
        public bool CaptureStackTrace { get; set; }

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
        public void RawExecute()
        {
            Script("SELECT 1");
        }
        [Benchmark]
        public async Task RawAsyncExecute()
        {
            await ScriptAsync("SELECT 1");
        }
        [Benchmark]
        public async Task AsyncExecute()
        {
            await scriptExecuter.ExecuteAsync("SELECT 1");
        }
    }
}
