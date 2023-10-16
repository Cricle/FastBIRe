using FastBIRe.Cdc.Events;
using FastBIRe.Cdc.Mssql;
using FastBIRe.Cdc.MySql;
using FastBIRe.Cdc.NpgSql;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using rsa;
using System.Numerics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FastBIRe.CdcSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var mssql = ConnectionProvider.GetDbMigration(DatabaseSchemaReader.DataSchema.SqlType.SqlServer, "test");
            SqlDependency.Start("Server=192.168.1.101;Uid=sa;Pwd=Syc123456.;Connection Timeout=2000;TrustServerCertificate=true;Database=test");
            var comm = new MssqlCdcManager(() => new DefaultScriptExecuter(mssql), TimeSpan.FromSeconds(5));
            var listen = await comm.GetCdcListenerAsync();
            listen.EventRaised += Vars_EventRaised;
            await listen.StartAsync();
            Console.ReadLine();
        }
        private static void Dep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            Console.WriteLine($"{e.Info},{e.Type},{e.Source}");
        }

        public static void Vars_EventRaised(object? sender, CdcEventArgs e)
        {
            switch (e)
            {
                case TableMapEventArgs tme:
                    {
                        Console.WriteLine($"TableMap:{tme.TableInfo?.DatabaseName}.{tme.TableInfo?.TableName} -> {tme.TableInfo?.Id}");
                    }
                    break;
                case InsertEventArgs iea:
                    {
                        foreach (var item in iea.Rows)
                        {
                            Console.WriteLine($"InsertData:{string.Join(",", item)}");
                        }
                    }
                    break;
                case UpdateEventArgs uea:
                    {
                        foreach (var item in uea.Rows)
                        {
                            Console.WriteLine($"UpdateData:{(item.BeforeRow==null?null:string.Join(",", item.BeforeRow))} -> {string.Join(",", item.AfterRow)}");
                        }
                    }
                    break;
                case DeleteEventArgs dea:
                    {
                        foreach (var item in dea.Rows)
                        {
                            Console.WriteLine($"DeleteData:{string.Join(",", item)}");
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}