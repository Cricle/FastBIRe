using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.AAMode;
using FastBIRe.Naming;
using FastBIRe.Timing;
using FastBIRe.Triggering;
using rsa;
using System.Data;

namespace FastBIRe.Mig
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //var sql=TriggerWriter.Default.CreateEffect(SqlType.MySql, "trigger1", TriggerTypes.BeforeInsert, "tb","dest",new EffectTriggerSettingItem[]
            //{
            //    new EffectTriggerSettingItem("a1","a1","a1"),
            //    new EffectTriggerSettingItem("a2","a2","a2"),
            //    new EffectTriggerSettingItem("a3","a3","a3"),
            //});
            //foreach (var item in sql)
            //{
            //    Console.WriteLine(item);
            //}
            var sqlType = SqlType.SqlServer;
            var dbName = "test";
            var migSer = ConnectionProvider.GetDbMigration(sqlType, dbName);
            var reader = migSer.Reader;
            var ach = reader.Table("guidang");
            var agg = reader.Table("juhe");
            var adm = EffectTableCreateAAModelHelper.Default;
            //var s = reader.Table("guidang");
            //var a = reader.Table("juhe");
            //var req = new EffectTableCreateAAModelRequest(s, a, new EffectTableSettingItem[]
            //{
            //    new EffectTableSettingItem(new DatabaseColumn
            //    {
            //        Name="a7"
            //    }.SetTypeDefault( sqlType,DbType.Int64))
            //});
            //adm.Apply(reader, req);
            //foreach (var item in req.Scripts)
            //{
            //    Console.WriteLine(item);
            //}
            var tw = TriggerWriter.Default;
            var wwTable = reader.Table("ww");
           var sqls= tw.CreateTimeExpand(sqlType, "triggerx", TriggerTypes.InsteadOfInsert, wwTable, new string[] { "qq" }, new TimeExpandHelper(sqlType));
            foreach (var item in sqls)
            {
                Console.WriteLine(item);
            }
            //var sqls = tw.CreateEffect(SqlType.MySql, "triggerx", TriggerTypes.BeforeInsert, "guidang", "juhe", new EffectTriggerSettingItem[]
            //{
            //    EffectTriggerSettingItem.Trigger("a7",sqlType)
            //});
            //foreach (var item in sqls)
            //{
            //    Console.WriteLine(item);
            //}
            //var table = reader.Table("qqx");

            //table.Columns[0].Name = "hello";
            //table.Columns[0].Id = "1";

            //var old = reader.Table("qqx");
            //old.Columns[0].Id = "1";
            //var res = CompareSchemas.FromTable(reader.DatabaseSchema.ConnectionString, sqlType, old, table).Execute();
            //Console.WriteLine(res);
        }
    }
}