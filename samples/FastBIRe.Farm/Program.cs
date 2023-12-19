using FastBIRe.AP.DuckDB;
using FastBIRe.Cdc;
using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.Events;
using FastBIRe.Cdc.Mssql;
using FastBIRe.Cdc.Mssql.Checkpoints;
using FastBIRe.Cdc.MySql;
using FastBIRe.Cdc.NpgSql;
using MySqlConnector;
using Npgsql.Replication;
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
        private static async Task<ICdcListener> CreateMssqlListener(FarmManager farm, ICheckpoint? checkpoint)
        {
            var mgr = new MssqlCdcManager(farm.SourceFarmWarehouse.ScriptExecuter);
            Console.WriteLine(await mgr.IsDatabaseSupportAsync());
            Console.WriteLine(await mgr.IsDatabaseCdcEnableAsync(databaseName));
            Console.WriteLine(await mgr.IsTableCdcEnableAsync(databaseName, tableName));
            if (checkpoint == null)
            {
                await mgr.TryEnableDatabaseCdcAsync(databaseName);
                await mgr.TryDisableTableCdcAsync(databaseName, tableName);
                await mgr.TryEnableTableCdcAsync(databaseName, tableName);
            }
            return await mgr.GetCdcListenerAsync(new MssqlGetCdcListenerOptions(
                TimeSpan.FromSeconds(5),
                farm.SourceFarmWarehouse.ScriptExecuter,
                checkpoint: checkpoint));
        }
        private static async Task<ICdcListener> CreatePostgresqlListner(FarmManager farm, ICheckpoint? checkpoint)
        {
            var rconn = new LogicalReplicationConnection(farm.SourceFarmWarehouse.Connection.ConnectionString + ";password=Syc123456.");
            await rconn.Open();
            var mgr = new PgSqlCdcManager(farm.SourceFarmWarehouse.ScriptExecuter);
            Console.WriteLine(await mgr.IsDatabaseSupportAsync());
            Console.WriteLine(await mgr.IsDatabaseCdcEnableAsync(databaseName));
            Console.WriteLine(await mgr.IsTableCdcEnableAsync(databaseName, tableName));
            if (checkpoint == null)
            {
                await mgr.TryDisableTableCdcAsync(databaseName, tableName);
                await mgr.TryEnableTableCdcAsync(databaseName, tableName);
            }
            return await mgr.GetCdcListenerAsync(PgSqlGetCdcListenerOptions.CreateDefault(rconn,
                databaseName, tableName, checkpoint: checkpoint));

        }
        private static async Task<ICdcListener> CreateMySqlListner(FarmManager farm, ICheckpoint? checkpoint)
        {
            var mysqlCfg = new MySqlConnectionStringBuilder(farm.SourceFarmWarehouse.Connection.ConnectionString);
            var mysqlCdcMgr = new MySqlCdcManager(farm.SourceFarmWarehouse.ScriptExecuter, MySqlCdcModes.Gtid);
            return await mysqlCdcMgr.GetCdcListenerAsync(new MySqlGetCdcListenerOptions(checkpoint, opt =>
            {
                opt.Port = (int)mysqlCfg.Port;
                opt.Hostname = mysqlCfg.Server;
                opt.Password = "Syc123456.";
                opt.Username = mysqlCfg.UserID;
                opt.Database = mysqlCfg.Database;
            }));
        }
        static async Task Main(string[] args)
        {
            var farm = FarmHelper.CreateFarm(tableName, false);
            await farm.DestFarmWarehouse.ScriptExecuter.ExecuteAsync("SET memory_limit='128MB';");
            farm.DestFarmWarehouse.ScriptExecuter.RegistScriptStated((o, e) =>
            {
                if (e.TryToKnowString(out var msg))
                {
                    Console.WriteLine(msg);
                }
            });
            var storage = new FolderCheckpointStorage(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, farm.SourceFarmWarehouse.SqlType.ToString(), "checkpoints"));
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
            var pkg = await storage.GetAsync(databaseName, tableName);
            var checkpoint = pkg?.CastCheckpoint<ICheckpoint>(MssqlCheckpointManager.Instance);
            if (!syncOk && checkpoint == null)
            {
                await MigrationDatasAsync(farm);
            }
            var listner = await CreateMssqlListener(farm, checkpoint);
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
                        await farm.DestFarmWarehouse.ScriptExecuter.ReadAsync($"SELECT * from {dSqlType.Wrap("juhe2")} WHERE _id = {dSqlType.WrapValue(Random.Shared.Next(0, int.MaxValue))};",
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