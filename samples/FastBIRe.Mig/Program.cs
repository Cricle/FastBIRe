using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Builders;
using DatabaseSchemaReader.SqlGen;
using DuckDB.NET.Data;
using FastBIRe.AAMode;
using FastBIRe.Cdc.Events;
using FastBIRe.Cdc.Mssql;
using FastBIRe.Cdc.Triggers;
using FastBIRe.Creating;
using FastBIRe.Store;
using rsa;
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
            var sqlType = SqlType.MySql;
            var dbName = "test-1";
            using (var dbct = ConnectionProvider.GetDbMigration(sqlType, null))
            {
                var ada = DatabaseCreateAdapter.Get(sqlType);
                var executeDbc = new DefaultScriptExecuter(dbct) { CaptureStackTrace = true };
                executeDbc.ScriptStated += DebugHelper.OnExecuterScriptStated;
                var dbExists = await executeDbc.ReadAsync<int>(ada.CheckDatabaseExists(dbName));
                if (dbExists.Count == 0)
                {
                    await executeDbc.ExecuteAsync(ada.CreateDatabase(dbName));
                }
            }
            var dbc = ConnectionProvider.GetDbMigration(sqlType, dbName);
            var executer = new DefaultScriptExecuter(dbc) { CaptureStackTrace = true };
            executer.ScriptStated += DebugHelper.OnExecuterScriptStated;
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
            await GoAsync(executer);
            Console.WriteLine(new TimeSpan(Stopwatch.GetTimestamp() - sw));
            Console.ReadLine();
            dataStore.Dispose();
        }
        private static async Task Orm(IScriptExecuter executer)
        {
            var res = await executer.ReadAsync<Data>("SELECT 1 AS a");
        }
        private static async Task MigTableAsync(string file, string tableName, DbConnection dbConnection, IScriptExecuter executer, IDataStore triggerDataStore)
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
                var table = new DatabaseTable { Name = tableHelper.TableName, PrimaryKey = new DatabaseConstraint { Name = tableHelper.PrimaryKeyName, TableName = tableHelper.TableName } };
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
                }
                return @new;
            });
            await executer.ExecuteBatchAsync(scripts);
            var table = tableHelper.Table;
            foreach (var item in vtb.Columns)
            {
                if (item.IX)
                {
                    scripts = tableHelper.CreateIndexScript(item.Name, true);
                    await executer.ExecuteBatchAsync(scripts);
                }
                else if (!item.PK)
                {
                    var idxName = tableHelper.IndexNameGenerator.Create(new[] { tableHelper.TableName, item.Name });
                    var idx = table.Indexes.FirstOrDefault(x => x.Name == idxName);
                    if (idx != null)
                    {
                        await executer.ExecuteBatchAsync(tableHelper.DropIndexScript(item.Name));
                    }
                }
            }
        }
        private static DuckDBConnection duckdbc;
        private static DefaultScriptExecuter duckExecuter;
        private static DatabaseTable table;
        private static TableWrapper wrapper;

        private static async Task GoAsync(IDbScriptExecuter executer)
        {
            if (File.Exists("guidang.duckdb"))
            {
                File.Delete("guidang.duckdb");
            }
            duckdbc = new DuckDBConnection("Data Source=guidang.duckdb");
            duckdbc.Open();
            var cdcExecuter = new DefaultScriptExecuter(executer.Connection);
            var reader = executer.CreateReader();
            table = reader.Table("guidang", ReadTypes.AllColumns & ~ReadTypes.Indexs);

            table.Columns.ForEach(x => x.IsAutoNumber = false);

            duckExecuter = new DefaultScriptExecuter(duckdbc);
            duckExecuter.ScriptStated += DebugHelper.OnExecuterScriptStated;
            var scripts = new DdlGeneratorFactory(SqlType.DuckDB).TableGenerator(table).Write();
            await duckExecuter.ExecuteAsync(scripts);

            var cdc = new TriggerCdcManager(cdcExecuter);
            await cdc.TryEnableTableCdcAsync(executer.Connection.Database, "guidang");
            var listner = await cdc.GetCdcListenerAsync(new TriggerGetCdcListenerOptions(cdcExecuter, TimeSpan.FromMilliseconds(500), 200, null,new string[]
            {
                "guidang_affect"
            }));
            listner.EventRaised += Listner_EventRaised;
            wrapper = new TableWrapper(table, SqlType.DuckDB, null);
            await listner.StartAsync();
        }

        private static async void Listner_EventRaised(object sender, CdcEventArgs e)
        {
            switch (e)
            {
                case InsertEventArgs iea:
                    {
                        using (var append = duckdbc.CreateAppender("guidang"))
                        {
                            foreach (var item in iea.Rows)
                            {
                                var row = append.CreateRow();
                                for (int i = 0; i < item.Count; i++)
                                {
                                    Add(row, item[i]);
                                }
                                row.EndRow();
                            }
                        }
                        Console.WriteLine($"Alread add {iea.Rows.Count}");
                    }
                    break;
                case UpdateEventArgs uea:
                    {
                        foreach (var item in uea.Rows)
                        {
                            var sql = wrapper.CreateUpdateByKeySql(item.AfterRow);
                            await duckExecuter.ExecuteAsync(sql);
                        }
                        Console.WriteLine($"Alread update {uea.Rows.Count}");
                    }
                    break;
                case DeleteEventArgs dea:
                    {
                        foreach (var item in dea.Rows)
                        {
                            var sql = wrapper.CreateDeleteByKeySql(item);
                            await duckExecuter.ExecuteAsync(sql);
                        }
                        Console.WriteLine($"Alread delete {dea.Rows.Count}");
                    }
                    break;
                default:
                    break;
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
                case decimal: row.AppendValue((double)(decimal)item); break;
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