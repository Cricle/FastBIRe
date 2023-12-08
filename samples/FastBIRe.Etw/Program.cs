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
                    builder.Column("id", DbType.Int32, isAutoNumber: true, identityIncrement: 2)
                        .Column("name", DbType.DateTime)
                        .Column("dt", DbType.DateTime, length: 14, nullable: false)
                        .Column("sc", DbType.Decimal, precision: 21, scale: 4);

                    builder.SetPrimaryKey("id");
                    builder.AddIndex("dt", orderDesc: true, isUnique: false);
                });
            var provider = builder.Build();
            var ctx = new FastBIReContext(conn, provider);
            conn.Open();
            var executer = new DefaultScriptExecuter(conn);
            executer.ScriptStated += OnScriptStated;
            var listner = new MyEventListener();
            listner.Listen();
            executer.CaptureStackTrace = false;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                var d = await executer.ReadOneAsync<int>("SELECT @a+@b;", args: new { a = 123, b = 23 });
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
