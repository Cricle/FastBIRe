using DatabaseSchemaReader.DataSchema;
using FastBIRe.QutoMode;
using MySqlConnector;
using System.Data;

namespace FastBIRe.Sample
{
    //sqlite wal模式
    internal class Program
    {
        static void Main(string[] args)
        {
            CompareM();
        }
        static DbMigration GetDbMigration()
        {
            var conn = new MySqlConnection("Server=192.168.1.95;Port=3306;Uid=root;Pwd=syc123;Connection Timeout=2000;Character Set=utf8;Database=fe0bcae73fa74568b54eae9d7b284e7f_data;");
            conn.Open();
            return new DbMigration(conn);
        }
        static void RunMigration()
        {
            var mig = GetDbMigration();
            var script = mig.CompareWithModify("Student", x =>
            {
                var col = x.FindColumn("Name");
                col.DbDataType = mig.Reader.FindDataTypesByDbType(DbType.Int32);
            }).Execute();
            Console.WriteLine(script);
        }
        static void CompareM()
        {
            var ser = new MigrationService(GetDbMigration());
            var d = ser.DbMigration.GetMergeHelper();
            var builder = new SourceTableColumnBuilder(d, "a", "b");
            var s = GetSourceDefine(builder);
            var str = ser.RunMigration("8ae26aa2-5def-4209-98fd-1002954ba963",
                new SourceTableDefine("d7e3e404-1eb1-4c93-9956-ec66030804e0", s),
                builder.CloneWith(s, def =>
                {
                    def.Id = def.Field;
                    return def;
                }));
            Console.WriteLine(str);
        }
        static void CreateTable()
        {
            var tableName = "test";
            var mig = GetDbMigration();
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
            Console.WriteLine(script);
        }
        static SourceTableColumnDefine[] GetSourceDefine(SourceTableColumnBuilder builder)
        {
            var defs = new SourceTableColumnDefine[]
            {
                builder.Method("记录时间","记录时间", ToRawMethod.Now,onlySet:true,type:builder.Type(DbType.DateTime),sourceNullable:false,destNullable:false),
                builder.Method("a1","a1", ToRawMethod.Count,type:builder.Type(DbType.String,255)),
                builder.Method("a2","a2", ToRawMethod.Count,type:builder.Type(DbType.Decimal,25,5)),
                builder.Method("a3","a3", ToRawMethod.Count,type:builder.Type(DbType.Decimal,25,5)),
                builder.Method("a4","a4", ToRawMethod.Count,type:builder.Type(DbType.Decimal,25,5)),
                builder.Method("a5","a5", ToRawMethod.Count,type:builder.Type(DbType.String,255)),
                builder.Method("a7","111aaaa7777", ToRawMethod.None,true,type:builder.Type(DbType.DateTime)),
                builder.Method("aaaa8","aaaa8", ToRawMethod.None,true,type:builder.Type(DbType.String,254)),
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
            CompileOptions? options = new CompileOptions { EffectTable = "8ae26aa2-5def-4209-98fd-1002954ba963_effect", IncludeEffectJoin = true };
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