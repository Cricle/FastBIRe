using DatabaseSchemaReader.DataSchema;
using FastBIRe.Timescale;
using System.Runtime.CompilerServices;

namespace FastBIRe.Sample.Functions
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(TimescaleHelper.Default.DropChunks("device"));
        }
    }
}