using BenchmarkDotNet.Attributes;
using Dapper;
using DatabaseSchemaReader.DataSchema;
using rsa;
using System.Data.Common;

namespace FastBIRe.Benchmarks.Actions
{
    [MemoryDiagnoser]
    public class OrmBenchmarks
    {
        DbConnection connection;
        IDbScriptExecuter scriptExecuter;
        [GlobalSetup]
        public void Setup()
        {
            connection = ConnectionProvider.GetDbMigration(SqlType.MySql, "sakila");
            scriptExecuter = new DefaultScriptExecuter(connection);
        }

        [Benchmark(Baseline = true)]
        public async Task DapperRun()
        {
            await connection.QueryAsync<AddressObject>("SELECT * FROM `address` limit 100;");
        }
        [Benchmark]
        public async Task FastBIReRun()
        {
            await scriptExecuter.ReadAsync<AddressObject>("SELECT * FROM `address` limit 100;");
        }
        public record class AddressObject
        {
            public int? address_id { get; set; }

            public string address { get; set; }

            public string address2 { get; set; }

            public string district { get; set; }

            public long? city_id { get; set; }

            public string postal_code { get; set; }

            public string phone { get; set; }

            public byte[] location { get; set; }

            public DateTime? last_update { get; set; }
        }

    }
}
