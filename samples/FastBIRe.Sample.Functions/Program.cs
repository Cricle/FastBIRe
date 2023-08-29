using FastBIRe.Timescale;

namespace FastBIRe.Sample.Functions
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(TimescaleViews.GetHypertable("device", false));
        }
    }
}