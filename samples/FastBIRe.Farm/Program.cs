using FastBIRe.AP.DuckDB;
using FastBIRe.Cdc;
using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.Events;
using FastBIRe.Cdc.MySql;
using FastBIRe.Cdc.MySql.Checkpoints;
using MySqlCdc;
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
            await farm.DestFarmWarehouse.ScriptExecuter.ExecuteAsync("SET memory_limit='64MB';");
            farm.DestFarmWarehouse.ScriptExecuter.RegistScriptStated((o, e) =>
            {
                if (e.State == ScriptExecutState.Executed)
                {
                    Console.WriteLine($"Execute \n{e.Script}");
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
            var state = ((MySqlCheckpoint?)pkg?.CastCheckpoint(MysqlCheckpointManager.Instance))?.ToGtidState();
            if (!syncOk && state == null)
            {
                await MigrationDatasAsync(farm);
            }
            var mysqlCdcMgr = new MySqlCdcManager(farm.SourceFarmWarehouse.ScriptExecuter, opt =>
            {
                opt.Port = (int)mysqlCfg.Port;
                opt.Hostname = mysqlCfg.Server;
                opt.Password = "Syc123456.";
                opt.Username = mysqlCfg.UserID;
                opt.Database = mysqlCfg.Database;
                opt.ServerId = 1;
                if (state != null)
                {
                    opt.Binlog = BinlogOptions.FromGtid(state);
                    Console.WriteLine("Loaded gtid {0}", state);
                }
            }, MySqlCdcModes.Gtid);
            var listner = await mysqlCdcMgr.GetCdcListenerAsync(new MySqlGetCdcListenerOptions(null));
            listner.AttachToDispatcher(eh);
            await listner.StartAsync();
            Console.WriteLine("Now listing data changes, q key is quit");
            while (Console.ReadKey().Key != ConsoleKey.Q) ;
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