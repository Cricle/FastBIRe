using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace FastBIRe.Etw
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.ReadLine();
            using var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();
            var listner = new MyEventListener();
            var executer = new DefaultScriptExecuter(conn);
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
                var d = await executer.ReadOneAsync<int>("SELECT @a+@b;", args: new { a = 123,b=23 });
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
