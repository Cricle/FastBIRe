using DatabaseSchemaReader.DataSchema;
using FastBIRe.AAMode;
using FastBIRe.Querying;
using FastBIRe.Timing;
using rsa;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastBIRe.Mig
{
    public class VObject
    {
        public string Id { get; set; }
    }
    public class VTable: VObject
    {
        public string Name { get; set; }

        public List<VColumn> Columns { get; set; }
    }
    public class VColumn : VObject
    {
        public string Name { get; set; }

        public DbType Type { get; set; }

        public int Length { get; set; } = 255;

        public int Scale { get; set; } = 2;

        public int Precision { get; set; } = 22;

        public bool Nullable { get; set; } = true;

        public bool PK { get; set; }

        public bool IX { get; set; }

        public void ToDatabaseColumn(DatabaseColumn column, SqlType sqlType)
        {
            column.Name = Name;
            column.Length = Length;
            column.Scale = Scale;
            column.Precision = Precision;
            column.SetTypeDefault(sqlType, Type);
        }
    }
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var sqlType = SqlType.SqlServer;
            var dbName = "test";
            var dbc = ConnectionProvider.GetDbMigration(sqlType, dbName);
            var executer = new DefaultScriptExecuter(dbc) { CaptureStackTrace = true };
            executer.ScriptStated += OnExecuterScriptStated;
            var inter = new AATableHelper("guidang", dbc);
            await GoAsync(executer, inter);
        }

        private static async Task GoAsync(IScriptExecuter executer, AATableHelper tableHelper)
        {
            var content = File.ReadAllText("Table.json");
            var vtb = JsonSerializer.Deserialize<VTable>(content,new JsonSerializerOptions { Converters =
                {
                    new JsonStringEnumConverter()
                }
            });
            var scripts = tableHelper.CreateTableOrMigrationScript(() =>
            {
                return new DatabaseTable
                {
                    Name="guidang",
                    Columns =
                    {
                        new DatabaseColumn
                        {
                            Name="a1",
                            Nullable=true
                        }.SetTypeDefault(SqlType.SQLite, DbType.Int32),
                    }
                };
            },(old, @new) =>
            {
                foreach (var item in vtb.Columns)
                {
                    var column = @new.FindColumn(item.Name);
                    if (column == null)
                    {
                        column = new DatabaseColumn();
                        item.ToDatabaseColumn(column, tableHelper.SqlType);
                        @new.AddColumn(column);
                    }
                    else
                    {
                        item.ToDatabaseColumn(column, tableHelper.SqlType);
                    }
                }
                @new.Columns.RemoveAll(x => !vtb.Columns.Any(y => y.Name == x.Name));
                return @new;
            });
            await executer.ExecuteAsync(scripts, default);
            scripts = tableHelper.EffectTableScript("juhe", new[] { "a1", "a2" });
            await executer.ExecuteAsync(scripts, default);
            scripts = tableHelper.EffectScript("juhe", "juhe_effect");
            await executer.ExecuteAsync(scripts, default);
            foreach (var item in vtb.Columns)
            {
                if (item.IX)
                {
                    scripts = tableHelper.CreateIndexScript(item.Name, true);
                    await executer.ExecuteAsync(scripts, default);
                }
            }
            var juheTable = tableHelper.DatabaseReader.Table("juhe");
            var builder = TableFieldLinkBuilder.From(tableHelper.DatabaseReader, "guidang", "juhe");
            var funcMapper = tableHelper.FunctionMapper;
            var query = tableHelper.MakeInsertQuery(MergeQuerying.Default, "juhe", new ITableFieldLink[]
            {
                builder.Expand("sa3",DefaultExpandResult.Expression("a3",funcMapper.SumC("{0}"))),
                builder.Expand("ca4",DefaultExpandResult.Expression("a4",funcMapper.CountC("{0}")))
            }, new ITableFieldLink[]
            {
                builder.Direct("a1","ja1"),
                builder.Direct("a2","ja2")
            });
            Console.WriteLine(query);
        }

        private static void OnExecuterScriptStated(object sender, ScriptExecuteEventArgs e)
        {
            if (e.State == ScriptExecutState.Executed || e.State == ScriptExecutState.Exception)
            {
                if (e.StackTrace!=null)
                {
                    var fr= DefaultScriptExecuter.GetSourceFrame(e.StackTrace);
                    if (fr != null)
                    {
                        Console.WriteLine($"{fr.GetFileName()}:{fr.GetFileLineNumber()}:{fr.GetFileColumnNumber()}");
                    }
                }
                ConsoleColor color = e.State == ScriptExecutState.Executed ? ConsoleColor.Green : ConsoleColor.Red;
                Console.ForegroundColor = color;
                Console.Write(e.State);
                Console.Write(": ");
                Console.ResetColor();
                Console.WriteLine(e.Script);
                if (e.State == ScriptExecutState.Exception)
                {
                    Console.WriteLine(e.ExecuteException);
                }
                Console.Write("RecordsAffected:");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(e.RecordsAffected);
                Console.ResetColor();

                Console.Write(", ExecutedTime: ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{e.ExecutionTime.Value.TotalMilliseconds:F4}ms");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("==============================================================");
                Console.ResetColor();
            }

        }
    }
}