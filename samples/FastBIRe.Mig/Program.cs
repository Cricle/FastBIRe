using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.AAMode;
using FastBIRe.Naming;
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
            var sqlType = SqlType.MySql;
            var dbName = "testc";
            var migSer = ConnectionProvider.GetDbMigration(sqlType, dbName);
            var reader = migSer.Reader;
            var ach = reader.Table("guidang");
            var agg = reader.Table("juhe");
            var adm = new EffectTableCreateAAModelHelper(new RegexNameGenerator("{0}_effect"), DefaultEffectTableKeyNameGenerator.Instance, StringComparison.Ordinal);
            var req = new EffectTableCreateAAModelRequest(ach, agg, new EffectTableSettingItem[]
            {
                new EffectTableSettingItem(new DatabaseColumn
                {
                    Name="a7",
                    DbDataType="varchar(22)"
                })
            });
            adm.Apply(reader, req);
            foreach (var item in req.Scripts)
            {
                Console.WriteLine(item);
            }
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