using FastBIRe.AP.DuckDB;
using FastBIRe.Cdc;
using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.Events;
using FastBIRe.Cdc.MySql;
using FastBIRe.Cdc.MySql.Checkpoints;
using MySqlConnector;
using System.Diagnostics;

namespace FastBIRe.Farm
{
    internal class Program
    {
        const string databaseName = "test-2";
        const string tableName = "juhe2";

        static async Task MigrationDatasAsync(FarmManager farm)
        {
            Console.WriteLine("Now delete all datas");
            await Elapsed(() => farm.DestFarmWarehouse.DeleteAsync(tableName));
            Console.WriteLine("Now syncing full datas");
            await Elapsed(() => farm.SyncDataAsync(tableName));
        }

        static async Task Main(string[] args)
        {
            var farm = FarmHelper.CreateFarm(tableName, false);
            await farm.DestFarmWarehouse.ScriptExecuter.ExecuteAsync("SET memory_limit='128MB';");
            farm.DestFarmWarehouse.ScriptExecuter.RegistScriptStated((o, e) =>
            {
                if (e.State == ScriptExecutState.Executed||e.State== ScriptExecutState.EndReading)
                {
                    Console.WriteLine($"Executed({e.ExecutionTime?.TotalMilliseconds:F3}ms) {e.Script}");
                }
            });
            var storage = new FolderCheckpointStorage(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "checkpoints"));
            Console.WriteLine("Now syncing structing");
            var syncResult = await farm.SyncAsync();
            var syncOk = false;
            if (syncResult == SyncResult.Modify)
            {
                await MigrationDatasAsync(farm);
                syncOk = true;
            }
            var handler = DuckDbCdcHandler.Create(farm.DestFarmWarehouse.ScriptExecuter, tableName, new CheckpointIdentity(databaseName, tableName), storage);
            var eh = new ChannelEventDispatcher<CdcEventArgs>(handler);
            await eh.StartAsync();
            var mysqlCfg = new MySqlConnectionStringBuilder(farm.SourceFarmWarehouse.Connection.ConnectionString);
            var pkg = await storage.GetAsync(databaseName, tableName);
            var checkpoint = pkg?.CastCheckpoint<MySqlCheckpoint>(MysqlCheckpointManager.Instance);
            if (!syncOk && checkpoint == null)
            {
                await MigrationDatasAsync(farm);
            }
            var mysqlCdcMgr = new MySqlCdcManager(farm.SourceFarmWarehouse.ScriptExecuter, MySqlCdcModes.Gtid);
            var listner = await mysqlCdcMgr.GetCdcListenerAsync(new MySqlGetCdcListenerOptions(null,checkpoint, opt =>
            {
                opt.Port = (int)mysqlCfg.Port;
                opt.Hostname = mysqlCfg.Server;
                opt.Password = "Syc123456.";
                opt.Username = mysqlCfg.UserID;
                opt.Database = mysqlCfg.Database;
            }));
            listner.AttachToDispatcher(eh);
            await listner.StartAsync();
            Console.WriteLine("Now listing data changes, q key is quit, a key to aggra, o to count");
            var dSqlType = farm.DestFarmWarehouse.SqlType;
            var fn = FunctionMapper.Get(dSqlType)!;
            while (true)
            {
                var k = Console.ReadKey();
                switch (k.Key)
                {
                    case ConsoleKey.Q:
                        goto Exit;
                    case ConsoleKey.A:
                        await farm.DestFarmWarehouse.ScriptExecuter.ReadAsync($"SELECT {fn.AverageC(dSqlType.Wrap("ja2"))},{fn.CountC(dSqlType.Wrap("sa3"))},{fn.AverageC(fn.Len(dSqlType.Wrap("ca4"))!)} from {dSqlType.Wrap("juhe2")} group by {fn.YearFull(dSqlType.Wrap("datetime"))};",
                            (o, e) =>
                            {
                                while (e.Reader.Read())
                                {
                                    Console.WriteLine(string.Join(", ", Enumerable.Range(0, e.Reader.FieldCount).Select(x => e.Reader[x])));
                                }
                                return Task.CompletedTask;
                            });
                        break;
                    case ConsoleKey.I:
                        await farm.DestFarmWarehouse.ScriptExecuter.ReadAsync($"SELECT * from {dSqlType.Wrap("juhe2")} WHERE _id = {dSqlType.WrapValue(Random.Shared.Next(0,int.MaxValue))};",
                            (o, e) =>
                            {
                                while (e.Reader.Read())
                                {
                                    Console.WriteLine(string.Join(", ", Enumerable.Range(0, e.Reader.FieldCount).Select(x => e.Reader[x])));
                                }
                                return Task.CompletedTask;
                            });
                        break;
                    case ConsoleKey.O:
                        var count = await farm.DestFarmWarehouse.ScriptExecuter.ReadOneAsync<int>($"SELECT COUNT(*) FROM {dSqlType.Wrap("juhe2")};");
                        Console.WriteLine("TotalSize:" + count);
                        break;
                    default:
                        break;
                }
            }
Exit:
            Console.ReadLine();
            farm.Dispose();
        }
        static async Task Elapsed(Func<Task> action)
        {
            var gc = GC.GetTotalMemory(false);
            var sw = Stopwatch.StartNew();

            await action();
            Console.WriteLine(sw.ElapsedMilliseconds.ToString("F3") + "ms");
            Console.WriteLine($"{(GC.GetTotalMemory(false) - gc) / 1024 / 1024.0:F5}MB");
        }
    }
}