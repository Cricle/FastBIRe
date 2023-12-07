using DatabaseSchemaReader.DataSchema;
using FastBIRe.Builders;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace FastBIRe.Etw
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var conn = new SqliteConnection("Data Source=a.db");
            var builder = new TablesProviderBuilder(SqlType.SQLite)
                .ConfigTable("a1", builder =>
                {
                    builder.ConfigColumn("id", DbType.Int32, isAutoNumber:true,identityIncrement:2);
                    builder.ConfigColumn("name", DbType.DateTime);
                    builder.ConfigColumn("dt", DbType.DateTime, length: 13, nullable: false);
                    builder.ConfigColumn("sc", DbType.Decimal, precision:22,scale:4);

                    builder.SetPrimaryKey(new[] { "id" });
                });
            var provider = builder.Build();
            var ctx = new FastBIReContext(conn, provider);
            conn.Open();
            var executer = new DefaultScriptExecuter(conn);
            var listner = new MyEventListener();
            executer.CaptureStackTrace = true;
            foreach (var item in EventSource.GetSources())
            {
                if (item.Name == "FastBIRe.ScriptExecuter")
                {
                    listner.EnableEvents(item, EventLevel.Verbose);
                }
            }
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10_000; i++)
            {
                var d = await executer.ReadOneAsync<int>("SELECT @a+@b;", args: new { a = 123, b = 23 });
            }
            Console.WriteLine(sw.Elapsed);
        }
    }
    public class MyEventListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            //Console.WriteLine(eventData.EventName);
            base.OnEventWritten(eventData);
        }
    }
}
