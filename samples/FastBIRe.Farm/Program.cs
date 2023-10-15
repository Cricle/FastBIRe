using DatabaseSchemaReader;
using DuckDB.NET.Data;
using MySqlConnector;
using System.Diagnostics;

namespace FastBIRe.Farm
{
    internal class Program
    {
        static FarmManager CreateFarm(bool listen)
        {
            var duck = new DuckDBConnection("Data source=a.db");
            var mysql = new MySqlConnection("Server=192.168.1.101;Port=3306;Uid=root;Pwd=Syc123456.;Connection Timeout=2000;Character Set=utf8;Database=test2");

            duck.Open();
            mysql.Open();

            var reader = new DatabaseReader(mysql) { Owner = mysql.Database };
            var table = reader.Table("juhe_effect");
            var mysqlExecuter = new DefaultScriptExecuter(mysql);
            var duckExecuter = new DefaultScriptExecuter(duck);
            if (listen)
            {
                mysqlExecuter.ScriptStated += DebugHelper.OnExecuterScriptStated!;
                duckExecuter.ScriptStated += DebugHelper.OnExecuterScriptStated!;
            }
            var selector = DefaultCursorRowHandlerSelector.Single(mysqlExecuter, duckExecuter, "juhe_effect");
            var sourceHouse = new FarmWarehouse(mysqlExecuter, selector,false);
            var destHouse = new DuckFarmWarehouse(duckExecuter, selector, false);
            return new FarmManager(table, sourceHouse, destHouse);
        }

        static async Task Main(string[] args)
        {
            var farm = CreateFarm(false);
            await farm.SyncAsync();
            //duckExecuter.ScriptStated -= DebugHelper.OnExecuterScriptStated!;
            //await Elp(() => farm.InsertAsync(DataSet()));
            //duckExecuter.ScriptStated += DebugHelper.OnExecuterScriptStated!;
            await farm.DestFarmWarehouse.CreateCheckPointIfNotExistsAsync("*");
            //await Elp(async () =>
            // {
            //     await farm.CheckPointAsync();
            // });

            _ = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay(Random.Shared.Next(500, 1000));
                    await farm.InsertAsync(DataSet());
                }
            });
            _ = Task.Factory.StartNew(async () =>
            {
                var checkPointfarm = CreateFarm(false);
                while (true)
                {
                    var bgCp = Stopwatch.GetTimestamp();
                    var results = await checkPointfarm.CheckPointAsync();
                    Console.WriteLine($"Complated check point take {Stopwatch.GetElapsedTime(bgCp).TotalMilliseconds:F4}ms, AffectedCount:{results.Sum(x => x.AffectedCount)}");
                    await Task.Delay(Random.Shared.Next(1000, 5000));
                }
            });
            Console.ReadLine();
        }
        static IEnumerable<IEnumerable<object>> DataSet()
        {
            for (int i = 0; i < Random.Shared.Next(500, 2000); i++)
            {
                yield return DataSetRow(i);
            }
        }
        static IEnumerable<object> DataSetRow(int i)
        {
            yield return i * 123.0;
            yield return i;
        }
        static void Elp(Action action)
        {
            var gc = GC.GetTotalMemory(false);
            var sw = Stopwatch.GetTimestamp();

            action();
            Console.WriteLine(new TimeSpan(Stopwatch.GetTimestamp() - sw).TotalMilliseconds.ToString("F3") + "ms");
            Console.WriteLine($"{(GC.GetTotalMemory(false) - gc) / 1024 / 1024.0:F5}MB");
        }
        static async Task Elp(Func<Task> action)
        {
            var gc = GC.GetTotalMemory(false);
            var sw = Stopwatch.GetTimestamp();

            await action();
            Console.WriteLine(new TimeSpan(Stopwatch.GetTimestamp() - sw).TotalMilliseconds.ToString("F3") + "ms");
            Console.WriteLine($"{(GC.GetTotalMemory(false) - gc) / 1024 / 1024.0:F5}MB");
        }
    }
    public class DuckFarmWarehouse : FarmWarehouse
    {
        private readonly DuckDBConnection duckDBConnection;
        public DuckFarmWarehouse(IDbScriptExecuter scriptExecuter, ICursorRowHandlerSelector cursorRowHandlerSelector, bool attackId)
            : base(scriptExecuter, cursorRowHandlerSelector, attackId)
        {
            duckDBConnection = (DuckDBConnection)scriptExecuter.Connection;
        }

        public DuckFarmWarehouse(IDbScriptExecuter scriptExecuter, DatabaseReader databaseReader, ICursorRowHandlerSelector cursorRowHandlerSelector, bool attackId)
            : base(scriptExecuter, databaseReader, cursorRowHandlerSelector, attackId)
        {
            duckDBConnection = (DuckDBConnection)scriptExecuter.Connection;
        }
        public override async Task InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<IEnumerable<object>> values, CancellationToken token = default)
        {
            using (var trans = duckDBConnection.BeginTransaction())
            {
                ulong id = 0;
                if (AttackId)
                {
                    id = await GetCurrentSeqAsync(token);
                }
                using (var appender = duckDBConnection.CreateAppender(tableName))
                {
                    foreach (var row in values)
                    {
                        id++;
                        var rowAppender = appender.CreateRow();
                        foreach (var item in row)
                        {
                            Add(rowAppender, item);
                        }
                        if (AttackId)
                        {
                            rowAppender.AppendValue(id);
                        }
                        rowAppender.EndRow();
                    }
                }
                if (AttackId)
                {
                    await UpdateCurrentSeqAsync(id, token);
                }
                await trans.CommitAsync(token);
            }
        }
        public override async Task InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<object> values, CancellationToken token = default)
        {
            using (var trans = duckDBConnection.BeginTransaction())
            {
                ulong id = 0;
                if (AttackId)
                {
                    id = await GetCurrentSeqAsync(token);
                }
                using (var appender = duckDBConnection.CreateAppender(tableName))
                {
                    id++;
                    var row = appender.CreateRow();
                    foreach (var item in values)
                    {
                        Add(row, item);
                    }
                    if (AttackId)
                    {
                        row.AppendValue(id);
                    }
                    row.EndRow();
                }
                if (AttackId)
                {
                    await UpdateCurrentSeqAsync(id, token);
                }
                await trans.CommitAsync(token);
            }
        }
        private static void Add(DuckDBAppenderRow row, object item)
        {
            switch (item)
            {
                case null: row.AppendNullValue(); break;
                case bool: row.AppendValue((bool)item); break;
                case string: row.AppendValue((string)item); break;
                case sbyte: row.AppendValue((sbyte)item); break;
                case short: row.AppendValue((short)item); break;
                case int: row.AppendValue((int)item); break;
                case long: row.AppendValue((long)item); break;
                case byte: row.AppendValue((byte)item); break;
                case ushort: row.AppendValue((ushort)item); break;
                case uint: row.AppendValue((uint)item); break;
                case ulong: row.AppendValue((ulong)item); break;
                case float: row.AppendValue((float)item); break;
                case double: row.AppendValue((double)item); break;
                case DateTime: row.AppendValue(((DateTime)item)); break;
                case DateOnly: row.AppendValue((DateOnly)item); break;
                case TimeOnly: row.AppendValue((TimeOnly)item); break;
                default:
                    break;
            }
        }

    }
}