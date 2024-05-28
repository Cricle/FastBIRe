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
        [Benchmark]
        public void SimpleParse()
        {
            var tb = new DataSchema(new[] { "_id", "datetime", "ja1", "ja2", "sa3", "ca4" }, new Type[] { typeof(long), typeof(DateTime), typeof(DateTime), typeof(long), typeof(decimal), typeof(string) });
            using (var stream = File.OpenRead("Resources/a.csv"))
            using (var reader = new StreamReader(stream))
            {
                var r = new CsvSimpleReader(tb);
                foreach (var item in r.EnumerableRows(reader))
                {
                }
            }
        }
    }
}
