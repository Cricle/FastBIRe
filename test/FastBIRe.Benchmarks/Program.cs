using BenchmarkDotNet.Running;

namespace FastBIRe.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();
        }
    }
}