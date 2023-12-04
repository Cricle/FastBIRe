using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace FastBIRe.Etw
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.ReadLine();
            using var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();
            var executer = new DefaultScriptExecuter(conn);
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100_000; i++)
            {
                executer.BeginTransaction();
                await executer.ExecuteAsync("SELECT 1;");
                executer.Commit();
            }
            Console.WriteLine(sw.Elapsed);
        }
    }
}
