using Dapper;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using FastBIRe.Data;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using rsa;
using System.Diagnostics;

namespace FastBIRe.Copying
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var sps = TableHelper.SplitSqlServerByGo(TableHelper.GetSqlServerDDLFunctionScripts());
            var mssql = new SqlConnection("Server=192.168.1.101;Uid=sa;Pwd=Syc123456.;Connection Timeout=2000;TrustServerCertificate=true;Database=test21");
            mssql.Open();
            var exec=new DefaultScriptExecuter(mssql);
            foreach (var item in sps)
            {
                await exec.ExecuteAsync(item);
            }
            return;
            var c = new MySqlConnection("Server=192.168.1.101;Port=3306;Uid=root;Pwd=Syc123456.;Connection Timeout=2000;Character Set=utf8;Database=ffd7dd6fd4014cad86d11265c6e878f4_project;");
            c.Open();
            var res = await TableHelper.MySql.DumpDatabaseCreateAsync(new DefaultScriptExecuter(c));
            await File.WriteAllLinesAsync("a.sql", res);
            //var s = Stopwatch.GetTimestamp();
            //using (var comm = c.CreateCommand())
            //{
            //    comm.CommandText = "SELECT * FROM `weather`;";
            //    using (var reader = await comm.ExecuteReaderAsync())
            //    using (var cop = CsvMirrorCopy.FromFile(reader, "weather.csv"))
            //    {
            //        await cop.CopyAsync();
            //    }
            //}
            //Console.WriteLine(new TimeSpan(Stopwatch.GetTimestamp() - s));
        }
    }
}