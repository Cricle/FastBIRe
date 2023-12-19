using DatabaseSchemaReader.DataSchema;
using FastBIRe;
using FastBIRe.Annotations;
using FastBIRe.Builders;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;

internal class Program
{
    static async Task Main(string[] args)
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        var executer = new DefaultScriptExecuter(conn);
        executer.ScriptStated += OnScriptStated;
        var ctx = conn.CreateTablesProviderBuilder()
            .ConfigStudent("student")
            .BuildContext(executer);
        await ctx.ExecuteMigrationScriptsAsync();

        for (int i = 0; i < 100; i++)
        {
            await executer.ExecuteAsync(CreateInsertSql());
        }
        for (int i = 0; i < 2; i++)
        {
            foreach (var item in executer.Enumerable<Student>("SELECT * FROM student WHERE id%10=@p1", new { p1 = 0 }))
            {
                //Console.WriteLine(item);
            }
        }
    }

    private static void OnScriptStated(object? sender, ScriptExecuteEventArgs e)
    {
        if (e.TryToKnowString(out var str))
        {
            Console.WriteLine(str);
        }
    }

    public static string CreateInsertSql()
    {
        return StudentModel.Instance.CreateInsertSql(SqlType.SQLite, "student", new Student
        {
            Age = Random.Shared.Next(),
            Flag = Random.Shared.Next(),
            Id = Random.Shared.Next(),
            Name = Random.Shared.Next() + "dsawdqawd"
        });
    }
}
[GenerateModel]
public record class Student
{
    [Key]
    [AutoNumber]
    [ColumnName("id")]
    public int Id { get; set; }

    [Index(IndexName = "dsioahgwodhq", IndexGroup = 1)]
    [Required]
    [ColumnName("name")]
    [MaxLength(256)]
    public string? Name { get; set; }

    [Index(IndexGroup = 1, IsDesc = true)]
    [Required]
    public long Age { get; set; }

    public long? Flag { get; set; }
}
