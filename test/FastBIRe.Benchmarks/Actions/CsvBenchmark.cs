using BenchmarkDotNet.Attributes;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace FastBIRe.Benchmarks.Actions
{
    [MemoryDiagnoser]
    [MemoryRandomization]
    public class CsvBenchmark
    {
        [Benchmark(Baseline = true)]
        public void CsvReaderHelper()
        {
            using (var stream = File.OpenRead("Resources/a.csv"))
            using (var reader = new StreamReader(stream))
            {
                var r = new CsvDataReader(new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)));
                while (r.Read())
                {
                    for (int i = 0; i < r.FieldCount; i++)
                    {
                        _ = r[i];
                    }
                }
            }
        }
    }
}
