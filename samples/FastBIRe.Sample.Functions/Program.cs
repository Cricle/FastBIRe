using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Sample.Functions
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var func = new FunctionMapper(SqlType.MySql);
            Console.WriteLine($@"
SELECT {func.DateAdd(func.Now(),"+22", DateTimeUnit.Day)} AS av
");
        }
    }
}