using DatabaseSchemaReader.DataSchema;
using rsa;

namespace FastBIRe.MinSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var sqlType = SqlType.MySql;
            var dbName = "testcw";
            const string 归档 = "guidang";
            const string 聚合 = "juhe";
            await ConnectionProvider.EnsureDatabaseCreatedAsync(sqlType, dbName);
            var conn = ConnectionProvider.GetDbMigration(sqlType, dbName);
            conn.EffectMode = true;
            conn.EffectTrigger = true;
            var builder = conn.GetColumnBuilder();
            var table = new SourceTableDefine(归档, GetSourceDefine(builder, sqlType, true));
            var tableSer = new TableService(conn);
            await tableSer.CreateTableIfNotExistsAsync(聚合);
            await tableSer.MigrationAsync(聚合, table.DestColumn);
            await tableSer.CreateTableIfNotExistsAsync(归档);
            await tableSer.MigrationAsync(归档, table.Columns);
            await tableSer.CreateTableIfNotExistsAsync(归档);
            await tableSer.MigrationAsync(聚合, table, false);
            await tableSer.SyncIndexAsync(聚合, table);

            var mr = conn.GetMergeHelper();
            CompileOptions opt = CompileOptions.EffectJoin("juhe_effect");
            opt.UseExpandField = true;
            table = new SourceTableDefine(归档, GetSourceDefine(builder, sqlType, false));
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
            var tr = TruncateHelper.Sql("juhe_effect", sqlType);
            Console.WriteLine(tr);
        }
        static List<SourceTableColumnDefine> GetSourceDefine(SourceTableColumnBuilder builder, SqlType sqlType, bool mig)
        {
            var f = new FunctionMapper(sqlType);
            f.Abs("1");
            var sumA2 = builder.Helper.ToRaw(ToRawMethod.Count, builder.SourceAliasQuto + "." + f.Quto("a2"), false);
            var @if = f.If($"{sumA2}/10=1", f.Value("succeed"), f.Value("fail"));
            var str = f.Bracket(@if);
            var lastDay = f.Concatenate(
                f.MinC(f.LastDay(builder.SourceAliasQuto + "." + f.Quto("记录时间"))),
                f.Value("num:"),
                f.RowNumber());
            var defs = new List<SourceTableColumnDefine>
            {
                builder.DateTime("记录时间","记录时间", ToRawMethod.Now,onlySet:true).AllNotNull(),
                builder.String("a2", "a1", ToRawMethod.Count),
                builder.Decimal("a2", "a2", ToRawMethod.Count),
                builder.Decimal("a3", "a3", ToRawMethod.Count),
                builder.Decimal("a4","a4", ToRawMethod.Count),
                builder.StringRaw("a5", lastDay,field:mig?"a5":null),
                builder.DateTime("a7","a7_m", ToRawMethod.Minute,isGroup:true).SetExpandDateTime(true,true),
                builder.DateTime("a7","a7_d", ToRawMethod.Day,isGroup:true).SetExpandDateTime(true,true),
                builder.DateTime("a7","a7_mon", ToRawMethod.Month,isGroup:true).SetExpandDateTime(true,true),
                builder.StringRaw("aaaa8",$"({str})",field:mig?"aaaa8":null,isGroup:false),
            };
            for (int i = 0; i < defs.Count; i++)
            {
                var item = defs[i];
                item.Id = i.ToString();
                if (item.DestColumn != null)
                {
                    item.DestColumn.Id = i.ToString();
                }
            }
            return defs;
        }
    }
}