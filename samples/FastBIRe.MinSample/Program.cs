using DatabaseSchemaReader.DataSchema;
using rsa;

namespace FastBIRe.MinSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var sqlType = SqlType.MySql;
            var dbName = "testc";
            const string 归档 = "guidang";
            const string 聚合 = "juhe";
            await ConnectionProvider.EnsureDatabaseCreatedAsync(sqlType, dbName);
            var conn = ConnectionProvider.GetDbMigration(sqlType, dbName);
            conn.EffectMode = true;
            conn.EffectTrigger = true;
            var builder = conn.GetColumnBuilder();
            var table = new SourceTableDefine(归档, GetSourceDefine(builder));
            var tableSer = new TableService(conn);
            await tableSer.CreateTableIfNotExistsAsync(聚合);
            await tableSer.MigrationAsync(聚合, table.DestColumn);
            await tableSer.CreateTableIfNotExistsAsync(归档);
            await tableSer.MigrationAsync(聚合, table);
            await tableSer.SyncIndexAsync(聚合, table);

            var mr = conn.GetMergeHelper();
            CompileOptions opt = CompileOptions.EffectJoin("juhe_effect");

            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine("===============");
            Console.ResetColor();
            Console.WriteLine();
            var insert = mr.CompileInsert(聚合, table, opt);
            Console.WriteLine(insert);
            Console.WriteLine();
            var update = mr.CompileUpdate(聚合, table, opt);
            Console.WriteLine(update);
            Console.WriteLine();
            var tr = TruncateHelper.Sql(opt?.EffectTable, sqlType);
            Console.WriteLine(tr);
        }
        static List<SourceTableColumnDefine> GetSourceDefine(SourceTableColumnBuilder builder)
        {
            var defs = new List<SourceTableColumnDefine>
            {
                builder.DateTime("记录时间","记录时间", ToRawMethod.Now,onlySet:true).AllNotNull(),
                builder.String("a1", "a1", ToRawMethod.Count),
                builder.Decimal("a2", "a2", ToRawMethod.Count),
                builder.Decimal("a3", "a3", ToRawMethod.Count),
                builder.Decimal("a4","a4", ToRawMethod.Count),
                builder.String("a5", "a5", ToRawMethod.DistinctCount),
                builder.DateTime("a7","111aaaa7777", ToRawMethod.Minute,isGroup:true).SetExpandDateTime(true,true),
                builder.String("aaaa8","aaaa8", ToRawMethod.None,true),
            };
            foreach (var item in defs)
            {
                item.Id = item.Field;
                item.DestColumn.Id = item.DestColumn.Field;
            }
            return defs;
        }
    }
}