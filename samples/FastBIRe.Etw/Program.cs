﻿using Microsoft.Data.Sqlite;
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
            for (int i = 0; i < 10; i++)
            {
                executer.BeginTransaction();
                await executer.ExecuteAsync("SELECT 1;");
                executer.Commit();
            }
            Console.WriteLine(sw.Elapsed);
        }
    }
    public class MyEventListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Console.WriteLine(eventData.EventName);
            base.OnEventWritten(eventData);
        }
    }
}
