using FastBIRe;

internal static class DebugHelper
{
    public static void OnExecuterScriptStated(object sender, ScriptExecuteEventArgs e)
    {
        if (e.State == ScriptExecutState.Executed || e.State == ScriptExecutState.Exception || e.State == ScriptExecutState.EndReading)
        {
            if (e.StackTrace != null)
            {
                var fr = DefaultScriptExecuter.GetSourceFrame(e.StackTrace);
                if (fr != null)
                {
                    Console.WriteLine($"{fr.GetFileName()}:{fr.GetFileLineNumber()}:{fr.GetFileColumnNumber()}");
                }
            }
            ConsoleColor color = e.State != ScriptExecutState.Exception ? ConsoleColor.Green : ConsoleColor.Red;
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
