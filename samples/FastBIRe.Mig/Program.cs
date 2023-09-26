using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.AAMode;
using FastBIRe.Naming;
using FastBIRe.Querying;
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
            var reader = ConnectionProvider.GetDbMigration(sqlType, dbName);
            var juhe = reader.Table("juhe");
            var guidang = reader.Table("guidang");
            var function = FunctionMapper.Get(sqlType);
            var groupLink = new ITableFieldLink[]
            {
                new DirectTableFieldLink(juhe.FindColumn("ja1"),guidang.FindColumn("a1")),
                new DirectTableFieldLink(juhe.FindColumn("ja2"),guidang.FindColumn("a2"))
            };
            var noGroupLink = new ITableFieldLink[]
            {
                new ExpandTableFieldLink(juhe.FindColumn("datetime"),DefaultExpandResult.Expression("datetime",function.Now())),
                new ExpandTableFieldLink(juhe.FindColumn("sa3"),DefaultExpandResult.Expression("a3",function.SumC("{0}"))),
                new ExpandTableFieldLink(juhe.FindColumn("ca4"),DefaultExpandResult.Expression("a3",function.CountC("{0}")))
            };
            var merquer = MergeQuerying.Default.Update(new MergeQueryUpdateRequest(sqlType, guidang, juhe, noGroupLink, groupLink)
            {
                EffectTable = reader.Table("juhe_effect"),
                UseEffectTable = true,
                IgnoreCompareFields =
                {
                    "datetime"
                }
            });
            Console.WriteLine(merquer);
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
            //var expandHelper = new TimeExpandHelper(sqlType);
            //var m = new TableExpandTimeAAModelHelper(expandHelper);
            //var req = new TableExpandTimeRequest("tx", new string[] { "aa", "bb" }, TimeTypes.ExceptSecond);
            //m.Apply(reader, req);
            //foreach (var item in req.Scripts)
            //{
            //    Console.WriteLine(item);
            //}
            //var tw = TriggerWriter.Default;
            //var wwTable = reader.Table("tx");
            //var sqls = tw.CreateTimeExpand(sqlType, "triggerx", TriggerTypes.InsteadOfInsert, wwTable, new string[] { "aa","bb" }, expandHelper);
            //foreach (var item in sqls)
            //{
            //    Console.WriteLine(item);
            //}
            //var sqls = tw.CreateEffect(SqlType.MySql, "triggerx", TriggerTypes.BeforeInsert, "guidang", "juhe", new EffectTriggerSettingItem[]
            //{
            //   EffectTriggerSettingItem.Trigger("a7",sqlType)
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