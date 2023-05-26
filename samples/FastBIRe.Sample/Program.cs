using DatabaseSchemaReader.DataSchema;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Npgsql;
using System.Data;
using System.Text.RegularExpressions;

namespace FastBIRe.Sample
{
    //影响表还是要原始类型
    /// </example>
    internal class Program
    {
        const string db = "testb";
        static void Main(string[] args)
        {
            //RealTrigger();
            //CompareM();
            RunQuery();
        }
        static MigrationService GetDbMigration(string? database)
        {
            //var conn = new NpgsqlConnection($"Host=127.0.0.1;Port=5432;Username=postgres;Password=355343{(string.IsNullOrEmpty(database) ? string.Empty : $";Database={database};")}");
            //var conn = new SqlConnection($"Server=192.168.1.95;Uid=sa;Pwd=Syc123456;Connection Timeout=2000;TrustServerCertificate=true{(string.IsNullOrEmpty(database) ? string.Empty : $";Database={database};")}");
            var conn = new MySqlConnection($"Server=127.0.0.1;Port=3306;Uid=root;Pwd=355343;Connection Timeout=2000;Character Set=utf8{(string.IsNullOrEmpty(database) ? string.Empty : $";Database={database};")}");
            //var conn = new MySqlConnection($"Server=192.168.1.95;Port=3307;Uid=root;Pwd=syc123;Connection Timeout=2000;Character Set=utf8{(string.IsNullOrEmpty(database) ? string.Empty : $";Database={database};")}");
            //var conn = new SqliteConnection($"{(string.IsNullOrEmpty(database) ? string.Empty : $"Data Source=C:\\Users\\huaji\\Desktop\\{database};")}");
            conn.Open();
            return new MigrationService(conn) { Logger = x => Console.WriteLine(x) };
        }
        static void IndexLen()
        {
            var mig = GetDbMigration(db);
            Console.WriteLine(IndexByteLenHelper
                .GetIndexByteLenAsync(mig.Connection, mig.SqlType).Result);
        }
        static void RealTrigger()
        {
            var ser = GetDbMigration(db);
            var d = ser.GetMergeHelper();
            var builder = new SourceTableColumnBuilder(d, "a", "b");
            var s = GetSourceDefine(builder);
            var sourceTable = new SourceTableDefine("d7e3e404-1eb1-4c93-9956-ec66030804e0", s);
            Console.WriteLine(new RealTriggerHelper().Create(
                "d7e3e404-1eb1-4c93-9956-ec66030804e0_triggery",
                "8ae26aa2-5def-4209-98fd-1002954ba963", 
                sourceTable, SqlType.MySql));
        }
        static void CompareM()
        {
            using (var createMig = GetDbMigration(null))
            {
                createMig.EnsureDatabaseCreatedAsync(db).GetAwaiter().GetResult();
            }
            var ser = GetDbMigration(db);
            var d = ser.GetMergeHelper();
            ser.ImmediatelyAggregate = false;
            ser.EffectMode = true;
            ser.EffectTrigger = false;
            var builder = new SourceTableColumnBuilder(d, "a", "b");
            var s = GetSourceDefine(builder,);
            var dt = GetDestDefine(builder);
            CreateTableIfNotExists(ser, "8ae26aa2-5def-4209-98fd-1002954ba963");
            var dstr = ser.RunMigration("8ae26aa2-5def-4209-98fd-1002954ba963", dt,
                builder.CloneWith(s, def =>
                {
                    def.Id = def.Field;
                    return def;
                }));
            ser.ExecuteNonQueryAsync(dstr).GetAwaiter().GetResult();
            CreateTableIfNotExists(ser, "7ae26aa2-5def-4209-98fd-1002954ba963");
            dstr = ser.RunMigration("7ae26aa2-5def-4209-98fd-1002954ba963", dt,
                builder.CloneWith(s, def =>
                {
                    def.Id = def.Field;
                    return def;
                }));
            ser.ExecuteNonQueryAsync(dstr).GetAwaiter().GetResult();
            CreateTableIfNotExists(ser, "d7e3e404-1eb1-4c93-9956-ec66030804e0");
            var sourceTable = new SourceTableDefine("d7e3e404-1eb1-4c93-9956-ec66030804e0", s);
            var str = ser.RunMigration("8ae26aa2-5def-4209-98fd-1002954ba963",
                sourceTable,
                builder.CloneWith(s, def =>
                {
                    def.Id = def.Field;
                    return def;
                }), true);
            ser.ExecuteNonQueryAsync(str).GetAwaiter().GetResult();
            str = ser.RunMigration("7ae26aa2-5def-4209-98fd-1002954ba963",
                sourceTable,
                builder.CloneWith(s, def =>
                {
                    def.Id = def.Field;
                    return def;
                }),true);
            ser.ExecuteNonQueryAsync(str).GetAwaiter().GetResult();

            _ = ser.SyncIndexAutoAsync("8ae26aa2-5def-4209-98fd-1002954ba963", sourceTable).GetAwaiter().GetResult();
            sourceTable.Columns[6].DestColumn.Length = 99999;
            sourceTable.Columns[6].Length = 99999;
            _ = ser.SyncIndexAutoAsync("7ae26aa2-5def-4209-98fd-1002954ba963", sourceTable).GetAwaiter().GetResult();
        }
        static void CreateTableIfNotExists(DbMigration mig, string tableName)
        {
            if (mig.Reader.TableExists(tableName))
            {
                return;
            }
            var migGen = mig.DdlGeneratorFactory.MigrationGenerator();
            var tb = new DatabaseTable
            {
                Name = tableName,
            };
            tb.AddColumn("_id", DbType.Int64, x => x.AddIdentity().AddPrimaryKey($"PK_{tableName}_id"));
            tb.AddColumn("记录时间", DbType.DateTime, x =>
            {
                x.Nullable = false;
                x.AddIndex($"IDX_{tableName}_记录时间");
            });
            var script = migGen.AddTable(tb);
            mig.ExecuteNonQueryAsync(script).GetAwaiter().GetResult();
        }
        static TableColumnDefine[] GetDestDefine(SourceTableColumnBuilder builder)
        {
            var defs = new TableColumnDefine[]
            {
                builder.Column("记录时间",type:builder.Type(DbType.DateTime),destNullable:false),
                builder.Column("a1",type:builder.Type(DbType.String,255),length:255),
                builder.Column("a2",type:builder.Type(DbType.Decimal,25,5),length:30),
                builder.Column("a3",type:builder.Type(DbType.Decimal,25,5),length:30),
                builder.Column("a4",type:builder.Type(DbType.Decimal,25,5),length:30),
                builder.Column("a5",type:builder.Type(DbType.String,255),length:255),
                builder.Column("111aaaa7777",type:builder.Type(DbType.DateTime),length:255).SetExpandDateTime(true),
                builder.Column("aaaa8",type:builder.Type(DbType.String,255),length:255),
            };
            foreach (var item in defs)
            {
                item.Id = item.Field;
            }
            return defs;
        }
        static SourceTableColumnDefine[] GetSourceDefine(SourceTableColumnBuilder builder,SqlType sqlType)
        {
            var f = new FunctionMapper(sqlType);
            var defs = new SourceTableColumnDefine[]
            {
                builder.Method("记录时间","记录时间", ToRawMethod.Now,onlySet:true,type:builder.Type(DbType.DateTime),sourceNullable:false,destNullable:false,length:8,destLength:8),
                builder.Method("a1", "a1", ToRawMethod.Count, type : builder.Type(DbType.String, 255),length:255,destLength:255),
                builder.Method("a2", "a2", ToRawMethod.Count, type : builder.Type(DbType.Decimal, 25, 5),length:30,destLength:30),
                builder.Method("a3", "a3", ToRawMethod.Count, type : builder.Type(DbType.Decimal, 25, 5),length:30,destLength:30),
                builder.Method("a4","a4", ToRawMethod.Count,type:builder.Type(DbType.Decimal,25,5),length:30,destLength:30),
                builder.Method("a5","a5", ToRawMethod.DistinctCount,type:builder.Type(DbType.String,255),length:255,destLength:255),
                builder.Method("a7","111aaaa7777", ToRawMethod.Minute,true,type:builder.Type(DbType.DateTime),destFieldType:builder.Type(DbType.String, 255),length:8,destLength:255).SetExpandDateTime(true),
                builder.MethodRaw("aaaa8","aaaa8",
                f.Concatenate(f.Now(),f.WrapValue("_")!,builder.Helper.ToRaw( ToRawMethod.Count,f.Quto("a1"),false)!)!
                ,true,type:builder.Type(DbType.String,255),destFieldType:builder.Type(DbType.String, 255),length:255,destLength:255),
            };
            foreach (var item in defs)
            {
                item.Id = item.Field;
            }
            return defs;
        }
        static void RunQuery()
        {
            var sqltype = SqlType.MySql;
            var t = new MergeHelper(sqltype);
            var builder = new SourceTableColumnBuilder(t, "a", "b");

            var cols = GetSourceDefine(builder);
            CompileOptions? options = CompileOptions.EffectJoin("8ae26aa2-5def-4209-98fd-1002954ba963_effect");
            var def = new SourceTableDefine("d7e3e404-1eb1-4c93-9956-ec66030804e0", cols);
            var si = t.CompileInsert("8ae26aa2-5def-4209-98fd-1002954ba963", def, options);
            var s = t.CompileUpdate("8ae26aa2-5def-4209-98fd-1002954ba963", def, options);

            Console.WriteLine(si);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(s);
        }
    }
}