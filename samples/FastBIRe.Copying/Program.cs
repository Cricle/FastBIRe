using Dapper;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.Data;
using rsa;
using System.Diagnostics;

namespace FastBIRe.Copying
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var sqlType = SqlType.SqlServer;
            var c = ConnectionProvider.GetDbMigration(sqlType, "test1");
            //var executer = new DefaultScriptExecuter(c);
            //var tb = sqlType.GetTableHelper()!;
            //if (sqlType== SqlType.PostgreSql)
            //{
            //    var pgScripts = TableHelper.GetPgDDLFunctionScripts();
            //    await executer.ExistsAsync(pgScripts);
            //}
            //var ddl =await tb.DumpTableCreateAsync("guidang", executer);
            //Console.WriteLine(ddl);
            var s = Stopwatch.GetTimestamp();
            using (var comm = c.CreateCommand())
            {
                comm.CommandText = "SELECT * FROM `weather`;";
                using (var reader = await comm.ExecuteReaderAsync())
                using (var cop = CsvMirrorCopy.FromFile(reader, "weather.csv"))
                {
                    await cop.CopyAsync();
                }
            }
            Console.WriteLine(new TimeSpan(Stopwatch.GetTimestamp() - s));
        }
    }
}