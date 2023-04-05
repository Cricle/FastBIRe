using DatabaseSchemaReader;
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
            RunQuery();
        }
        static DbMigration GetDbMigration()
        {
            var conn = new MySqlConnection("Server=127.0.0.1;Port=3306;Uid=root;Pwd=355343;Connection Timeout=2000;Character Set=utf8;Database=sakila;");
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
        static void RunQuery()
        {
            var sqltype = SqlType.MySql;
            var t = new MergeHelper(sqltype);
            var builder = new SourceTableColumnBuilder(t, "a", "b");

            var cols = new SourceTableColumnDefine[]
            {
                builder.Method("ObsTime","ObsTime", ToRawMethod.Now,onlySet:true),
                builder.Method("Temp","Temp", ToRawMethod.Count),
                builder.Method("FeelsLike","FeelsLike", ToRawMethod.Count),
                builder.Method("Cloud","Cloud", ToRawMethod.None,true,type:builder.Type( DbType.Double)),
                builder.Method("Dew","Dew", ToRawMethod.None,true,type:builder.Type( DbType.Double)),
            };
            t.WhereItems = new WhereItem[]
            {
                builder.WhereRaw("Cloud", ToRawMethod.None,"123")
            };
            CompileOptions? options = new CompileOptions { EffectTable= "weather1_effect", IncludeEffectJoin=true };
            var def = new SourceTableDefine("weather", cols);
            var si = t.CompileInsert("weather1", def, options);
            var s = t.CompileUpdate("weather1", def, options);

            Console.WriteLine(si);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(s);
        }
    }
}