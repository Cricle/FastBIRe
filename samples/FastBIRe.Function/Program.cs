using DatabaseSchemaReader.DataSchema;
using System.Data;

namespace FastBIRe.Function
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var dbTypes = new SqlType[] { SqlType.MySql, SqlType.SQLite, SqlType.PostgreSql, SqlType.SqlServer };
            foreach (var item in dbTypes)
            {
                var fun = FunctionMapper.Get(item)!;
                Console.WriteLine($"====={item}====");
                Console.WriteLine("SELECT "+fun.DateDifMonth("'2022-01-10 09:39:00'", "'2023-10-10 10:40:30'"));
            }
        }
    }
}