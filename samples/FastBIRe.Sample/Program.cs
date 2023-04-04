using DatabaseSchemaReader.DataSchema;
using MySqlConnector;
using System.Data;

namespace FastBIRe.Sample
{
    //sqlite wal模式
    internal class Program
    {
        static void Main(string[] args)
        {
            RunMigration();
        }
        static void RunMigration()
        {
            using (var conn = new MySqlConnection("Server=192.168.1.95;Port=3306;Uid=root;Pwd=syc123;Connection Timeout=2000;Character Set=utf8;Database=sakila;"))
            {
                var mig = new DbMigration(conn);
                var script = mig.CompareWithModify("actor", x =>
                {
                    var col = x.FindColumn("first_name");
                    col.DbDataType = mig.Reader.FindDataTypesByDbType(DbType.Int32);
                })?.Execute();
                Console.WriteLine(script);
            }
        }
        static void RunQuery()
        {
            var sqltype = SqlType.MySql;
            var t = new MergeHelper(sqltype);
            var builder = new SourceTableColumnBuilder(t, "a", "b");

            var cols = new SourceTableColumnDefine[]
            {
                builder.Method("记录时间","记录时间", ToRawMethod.Now,onlySet:true),
                builder.Method("a1","a1", ToRawMethod.Count),
                builder.Method("a2","a2", ToRawMethod.Count),
                builder.Method("a3","a3", ToRawMethod.Count),
                builder.Method("a4","a4", ToRawMethod.Count),
                builder.Method("a5","a5", ToRawMethod.Count),
                builder.Method("a7","111aaaa7777", ToRawMethod.None,true,type:builder.GetRawType(DbType.DateTime)),
                builder.Method("aaaa8","aaaa8", ToRawMethod.None,true,type:builder.GetRawType(DbType.String,"255")),
            };
            CompileOptions? options = null;//new CompileOptions { EffectTable = "8ae26aa2-5def-4209-98fd-1002954ba963_effect", IncludeEffectJoin = true };
            var def = new SourceTableDefine("d7e3e404-1eb1-4c93-9956-ec66030804e0", cols);
            var si = t.CompileInsert("8ae26aa2-5def-4209-98fd-1002954ba963", def, options);
            var s = t.CompileUpdate("8ae26aa2-5def-4209-98fd-1002954ba963", def, options);

            var create = new TableHelper(sqltype).CreateTable("8ae26aa2-5def-4209-98fd-1002954ba963_effect", cols.Where(x => x.IsGroup));

            Console.WriteLine(si);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(s);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(create);
        }
    }
}