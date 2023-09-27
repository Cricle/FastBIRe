using DatabaseSchemaReader.DataSchema;
using FastBIRe.AAMode;
using FastBIRe.Querying;
using FastBIRe.Timing;
using rsa;

namespace FastBIRe.Mig
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var sqlType = SqlType.SqlServer;
            var dbName = "test";
            var dbc = ConnectionProvider.GetDbMigration(sqlType, dbName);
            var executer = new DefaultScriptExecuter(dbc);
            executer.ScriptStated += OnExecuterScriptStated;
            var inter = new AATableHelper("guidang", dbc);
            await GoAsync(executer, inter);
        }

        private static async Task GoAsync(IScriptExecuter executer, AATableHelper tableHelper)
        {
            var scripts = tableHelper.EffectTableScript("juhe", new[] { "a1", "a2" });
            await executer.ExecuteAsync(scripts, default);
            scripts = tableHelper.EffectScript("juhe", "juhe_effect");
            await executer.ExecuteAsync(scripts, default);
            scripts = tableHelper.CreateIndexScript("a1",true);
            await executer.ExecuteAsync(scripts, default);
            var juheTable = tableHelper.DatabaseReader.Table("juhe");
            var builder = TableFieldLinkBuilder.From(tableHelper.DatabaseReader, "guidang", "juhe");
            var funcMapper = tableHelper.FunctionMapper;
            var query = tableHelper.QueryInsert(MergeQuerying.Default, "juhe", new ITableFieldLink[]
            {
                builder.Expand("sa3",DefaultExpandResult.Expression("a3",funcMapper.SumC("{0}"))),
                builder.Expand("ca4",DefaultExpandResult.Expression("a4",funcMapper.CountC("{0}")))
            }, new ITableFieldLink[]
            {
                builder.Direct("a1","ja1"),
                builder.Direct("a2","ja2")
            });
            Console.WriteLine(query);
        }

        private static void OnExecuterScriptStated(object sender, ScriptExecuteEventArgs e)
        {
            if (e.State == ScriptExecutState.Executed || e.State == ScriptExecutState.Exception)
            {
                ConsoleColor color = e.State == ScriptExecutState.Executed ? ConsoleColor.Green : ConsoleColor.Red;
                Console.ForegroundColor = color;
                Console.Write(e.State);
                Console.Write(": ");
                Console.ResetColor();
                Console.WriteLine(e.Script);
                if (e.State == ScriptExecutState.Exception)
                {
                    Console.WriteLine(e.ExecuteException);
                }
                Console.Write("RecordsAffected:");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(e.RecordsAffected);
                Console.ResetColor();

                Console.Write(", ExecutedTime: ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{e.ExecutionTime.Value.TotalMilliseconds:F4}ms");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("==============================================================");
                Console.ResetColor();
            }

        }
    }
}