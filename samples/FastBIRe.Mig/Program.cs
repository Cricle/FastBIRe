﻿using DatabaseSchemaReader.DataSchema;
using FastBIRe.AAMode;
using FastBIRe.Comparing;
using FastBIRe.Creating;
using FastBIRe.Querying;
using FastBIRe.Timing;
using rsa;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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

        public bool AI { get; set; }

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
            var sw = Stopwatch.GetTimestamp();
            var sqlType = SqlType.SQLite;
            var dbName = "test1";
            var dbc = ConnectionProvider.GetDbMigration(sqlType, dbName);
            var executer = new DefaultScriptExecuter(dbc) { CaptureStackTrace = true };
            executer.ScriptStated += OnExecuterScriptStated;
            await executer.ReadAsync("SELECT 1", (o, e) =>
            {
                while (e.Reader.Read())
                {

                }
                return Task.CompletedTask;
            },default);
            var inter = new AATableHelper("guidang", dbc);
            var r = inter.Table.Triggers;
            await MigTableAsync("GuiDangTable.json", "guidang", dbc, executer);
            await MigTableAsync("JuHeTable.json", "juhe", dbc, executer);
            await GoAsync(executer, inter);
            Console.WriteLine(new TimeSpan(Stopwatch.GetTimestamp()-sw));
        }

        private static async Task MigTableAsync(string file,string tableName,DbConnection dbConnection,IScriptExecuter executer)
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
                        item.ToDatabaseColumn(column, tableHelper.SqlType);
                        @new.AddColumn(column);
                    }
                    else
                    {
                        item.ToDatabaseColumn(column, tableHelper.SqlType);
                    }
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