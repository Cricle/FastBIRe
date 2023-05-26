using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Sample.Functions
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var func = new FunctionMapper(SqlType.MySql);
            Console.WriteLine($@"
SELECT {func.Bracket(func.Replace("name","1","2", func.WrapValue("newText")))} AS av
from student order by `name`;
");
        }
    }
}