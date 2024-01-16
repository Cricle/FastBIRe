using BenchmarkDotNet.Attributes;
using Dapper;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.Annotations;
using Microsoft.Diagnostics.Runtime.Utilities;
using rsa;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
[module: DapperAot]
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
        public async Task FastBIReOutterRun()
        {
            using (var result = await scriptExecuter.ReadAsync("SELECT * FROM `address` limit 100;"))
            {
                var reader = result.Args.Reader;
                while (reader.Read())
                {
                    result.Read<AddressObject>();
                }
            }
        }
        [Benchmark]
        public async Task FastBIReAsynIre()
        {
            await foreach (var item in scriptExecuter.EnumerableAsync<AddressObject>("SELECT * FROM `address` limit 100;"))
            {

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
