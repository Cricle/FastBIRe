using BenchmarkDotNet.Attributes;
using Dapper;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.Annotations;
using rsa;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

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
            _ = AddressObjectModel.Instance;
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
        [Benchmark]
        public async Task FastBIReEnumerableRun()
        {
            await scriptExecuter.EnumerableAsync<AddressObject>("SELECT * FROM `address` limit 100;", x =>
            {

            });
        }
        [Benchmark]
        public async Task FastBIReNativeRun()
        {
            using (var comm=connection.CreateCommand())
            {
                comm.CommandText = "SELECT * FROM `address` limit 100;";
                using (var reader=await comm.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        _ = AddressObjectModel.Instance.To(reader);
                    }
                }
            }
        }
    }
    [GenerateModel]
    public record struct AddressObject
    {
        public int? address_id { get; set; }

        public string? address { get; set; }

        public string? address2 { get; set; }

        public string? district { get; set; }

        public long? city_id { get; set; }

        public string? postal_code { get; set; }

        public string? phone { get; set; }

        public DateTime? last_update { get; set; }
    }

}
