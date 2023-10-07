using DatabaseSchemaReader.DataSchema;
using FastBIRe.Data;
using rsa;
using System.Diagnostics;

namespace FastBIRe.Copying
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var sqlType = SqlType.MySql;
            var c = ConnectionProvider.GetDbMigration(sqlType, "sakila");
            var s = Stopwatch.GetTimestamp();
            using (var comm = c.CreateCommand())
            {
                comm.CommandText = "SELECT * FROM `weather`;";
                using (var reader = await comm.ExecuteReaderAsync())
                using (var cop = CsvMirrorCopy.FromFile(reader, "weather.csv"))
                {
                    await cop.CopyAsync();
                }
            }
            Console.WriteLine(new TimeSpan(Stopwatch.GetTimestamp() - s));
        }
    }
}