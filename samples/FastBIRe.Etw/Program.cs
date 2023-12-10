using DatabaseSchemaReader.DataSchema;
using FastBIRe.Builders;
using MySqlConnector;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace FastBIRe.Etw
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var conn = new MySqlConnection("host=192.168.1.101;user id=root;password=Syc123456.;database=ttt");
            var builder = new TablesProviderBuilder(SqlType.MySql)
                .ConfigTable("a1", builder =>
                {
                    builder.Column("id", DbType.Int32, isAutoNumber: true)
                        .StringColumn("name",1)
                        .DateTimeColumn("dt", nullable: false)
                        .Column("sc", DbType.Decimal, precision: 21, scale: 4);

                    builder.SetPrimaryKey("id");
                    builder.AddIndex("dt", orderDesc: true);
                });
            conn.Open();
            var ctx = builder.BuildContext(conn);
            //executer.ScriptStated += OnScriptStated;
            await ctx.ExecuteMigrationScriptsAsync();
            var listner = new MyEventListener();
            listner.Listen();
            var sw = Stopwatch.StartNew();
            //await executer.ReadOneAsync<IList<int>>("SELECT 1,2,3,4;", args: new { bs = new byte[] { 1, 2, 3, 4 } });
            for (int i = 0; i < 1; i++)
            {
                var d = await ctx.Executer.ReadRowsAsync<int>("SELECT 1,2,3,4;", args: new { bs = new byte[] { 1, 2, 3, 4 } });
            }
            Console.WriteLine(sw.Elapsed);
        }

        private static void OnScriptStated(object? sender, ScriptExecuteEventArgs e)
        {
            var str = e.ToKnowString();
            if (!string.IsNullOrEmpty(str))
            {
                Console.WriteLine(str);
            }
        }
    }
    public class MyEventListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            //Console.WriteLine(eventData.EventName);
            base.OnEventWritten(eventData);
        }
        public void Listen()
        {
            foreach (var item in EventSource.GetSources())
            {
                if (item.Name == "FastBIRe.ScriptExecuter")
                {
                    EnableEvents(item, EventLevel.Verbose);
                }
            }
        }
    }
}
