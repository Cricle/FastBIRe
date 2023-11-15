using BenchmarkDotNet.Running;

namespace FastBIRe.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var cr = new CsvBenchmark();
            //cr.SimpleParse();
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();
        }
    }
}