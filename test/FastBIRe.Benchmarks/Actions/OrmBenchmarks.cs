using BenchmarkDotNet.Attributes;
using Dapper;
using DatabaseSchemaReader.DataSchema;
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
            RecordToObjectManager<AddressObject>.SetRecordToObject(AddressRecordToObject.Instance);
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
        class AddressRecordToObject : IRecordToObject<AddressObject>
        {
            public static readonly AddressRecordToObject Instance = new AddressRecordToObject();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AddressObject? To(IDataRecord record)
            {
                var or1 = record.GetOrdinal("address_id");
                var or2 = record.GetOrdinal("address");
                var or3 = record.GetOrdinal("address2");
                var or4 = record.GetOrdinal("district");
                var or5 = record.GetOrdinal("city_id");
                var or6 = record.GetOrdinal("postal_code");
                var or7 = record.GetOrdinal("phone");
                var or8 = record.GetOrdinal("location");
                var or9 = record.GetOrdinal("last_update");
                return new AddressObject
                {
                    address_id = record.IsDBNull(or1) ? null : record.GetInt32(or1),
                    address = record.IsDBNull(or2) ? null : record.GetString(or2),
                    address2 = record.IsDBNull(or3) ? null : record.GetString(or3),
                    district = record.IsDBNull(or4) ? null : record.GetString(or4),
                    city_id = record.IsDBNull(or5) ? null : record.GetInt64(or5),
                    postal_code = record.IsDBNull(or6) ? null : record.GetString(or6),
                    phone = record.IsDBNull(or7) ? null : record.GetString(or7),
                    location = record.IsDBNull(or8) ? null : (byte[])record[or8],
                    last_update = record.IsDBNull(or9) ? null : record.GetDateTime(or9),
                };
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IList<AddressObject?> ToList(IDataReader reader)
            {
                var obj = new List<AddressObject?>();
                while (reader.Read())
                {
                    obj.Add(To(reader));
                }
                return obj;
            }
        }
        public record class AddressObject
        {
            public int? address_id { get; set; }

            public string? address { get; set; }

            public string? address2 { get; set; }

            public string? district { get; set; }

            public long? city_id { get; set; }

            public string? postal_code { get; set; }

            public string? phone { get; set; }

            public byte[]? location { get; set; }

            public DateTime? last_update { get; set; }
        }

    }
}
