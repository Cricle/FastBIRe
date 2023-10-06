using DatabaseSchemaReader.DataSchema;
using FastBIRe.AAMode;
using FastBIRe.Querying;
using FastBIRe.Store;
using FastBIRe.Timing;
using rsa;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastBIRe.Mig
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var sqlType = SqlType.SQLite;
            var dbName = "test1";
            var dbc = ConnectionProvider.GetDbMigration(sqlType, dbName);
            var executer = new DefaultScriptExecuter(dbc) { CaptureStackTrace = true };
            executer.ScriptStated += OnExecuterScriptStated;
            await Orm(executer);
            var inter = new AATableHelper("guidang", dbc);
            var sw = Stopwatch.GetTimestamp();
            var store = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "triggers");
            if (!Directory.Exists(store))
            {
                Directory.CreateDirectory(store);
            }
            var fipath = Path.Combine(store, "cache.zip");
            var dataStore = ZipDataStore.FromFile("triggers", fipath);
            inter.TriggerDataStore = dataStore;
            await MigTableAsync("GuiDangTable.json", "guidang", dbc, executer, inter.TriggerDataStore);
            await MigTableAsync("JuHeTable.json", "juhe", dbc, executer, inter.TriggerDataStore);
            await GoAsync(executer, inter);
            Console.WriteLine(new TimeSpan(Stopwatch.GetTimestamp() - sw));
            dataStore.Dispose();
        }
        private static async Task Orm(IScriptExecuter executer)
        {
            var res = await executer.ReadAsync<Data>("SELECT 1 AS a");
        }
        private static async Task MigTableAsync(string file,string tableName,DbConnection dbConnection,IScriptExecuter executer,IDataStore triggerDataStore)
        {
            var content = File.ReadAllText(file);
            var vtb = JsonSerializer.Deserialize<VTable>(content, new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            });
            var tableHelper = new AATableHelper(tableName, dbConnection);
            tableHelper.TriggerDataStore = triggerDataStore;
            var scripts = tableHelper.CreateTableOrMigrationScript(() =>
            {
                var table = new DatabaseTable { Name = tableHelper.TableName,PrimaryKey=new DatabaseConstraint { Name = tableHelper.PrimaryKeyName,TableName= tableHelper.TableName } };
                foreach (var item in vtb.Columns)
                {
                    var column = new DatabaseColumn();
                    item.ToDatabaseColumn(column, tableHelper.SqlType);
                    table.AddColumn(column);
                    if (item.PK)
                    {
                        table.PrimaryKey.AddColumn(column);
                    }
                    if (item.AI)
                    {
                        column.AddIdentity();
                    }
                }
                return table;
            }, (old, @new) =>
            {
                foreach (var item in vtb.Columns)
                {
                    var column = @new.FindColumn(item.Name);
                    if (column == null)
                    {
                        column = new DatabaseColumn();
                        @new.AddColumn(column);
                    }
                    item.ToDatabaseColumn(column, tableHelper.SqlType);
                    if (column.DataType.IsDateTime)
                    {
                        var result = tableHelper.TimeExpandHelper.Create(column.Name, TimeTypes.ExceptSecond);
                        foreach (var res in result)
                        {
                            @new.AddColumn(res.Name, DbType.DateTime);
                        }
                    }
                }
                return @new;
            });
            await executer.ExecuteAsync(scripts);

            foreach (var item in vtb.Columns)
            {
                if (item.IX)
                {
                    scripts = tableHelper.CreateIndexScript(item.Name, true);
                    await executer.ExecuteAsync(scripts);
                }
            }
            var expandColumns = vtb.Columns.Where(x => x.Type == DbType.DateTime).Select(x=>x.Name).ToList();
            scripts = tableHelper.ExpandTimeMigrationScript(expandColumns);
            await executer.ExecuteAsync(scripts);
            scripts = tableHelper.ExpandTriggerScript(expandColumns);
            await executer.ExecuteAsync(scripts);
        }

        private static async Task GoAsync(IScriptExecuter executer, AATableHelper tableHelper)
        {
            var scripts = tableHelper.EffectTableScript("juhe", new[] { "a1", "a2" });
            await executer.ExecuteAsync(scripts, default);
            scripts = tableHelper.EffectScript("juhe", "juhe_effect");
            await executer.ExecuteAsync(scripts, default);
            var builder = TableFieldLinkBuilder.From(tableHelper.DatabaseReader, "guidang", "juhe");
            var funcMapper = tableHelper.FunctionMapper;
            var query = tableHelper.MakeInsertQuery(MergeQuerying.Default, "juhe", new ITableFieldLink[]
            {
                builder.Expand("sa3",DefaultExpandResult.Expression("a3",funcMapper.SumC("{0}"))),
                builder.Expand("ca4",DefaultExpandResult.Expression("a4",funcMapper.CountC("{0}")))
            }, new ITableFieldLink[]
            {
                builder.Direct("ja1","a1"),
                builder.Direct("ja2","a2")
            });
            Console.WriteLine(query);
        }

        private static void OnExecuterScriptStated(object sender, ScriptExecuteEventArgs e)
        {
            if (e.State == ScriptExecutState.Executed || e.State == ScriptExecutState.Exception||e.State== ScriptExecutState.EndReading)
            {
                if (e.StackTrace!=null)
                {
                    var fr= DefaultScriptExecuter.GetSourceFrame(e.StackTrace);
                    if (fr != null)
                    {
                        Console.WriteLine($"{fr.GetFileName()}:{fr.GetFileLineNumber()}:{fr.GetFileColumnNumber()}");
                    }
                }
                ConsoleColor color = e.State !=  ScriptExecutState.Exception ? ConsoleColor.Green : ConsoleColor.Red;
                Console.ForegroundColor = color;
                Console.Write(e.State);
                Console.Write(": ");
                Console.ResetColor();
                Console.WriteLine(e.Script);
                if (e.State == ScriptExecutState.Exception)
                {
                    Console.WriteLine(e.ExecuteException);
                }
                if (e.RecordsAffected != null)
                {
                    Console.Write("RecordsAffected:");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(e.RecordsAffected);
                    Console.ResetColor();
                    Console.Write(", ");
                }

                Console.Write("ExecutedTime: ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($"{e.ExecutionTime?.TotalMilliseconds:F4}ms ");
                Console.ResetColor();

                Console.Write(", FullTime: ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{e.FullTime?.TotalMilliseconds:F4}ms");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("==============================================================");
                Console.ResetColor();
            }

        }
    }
}