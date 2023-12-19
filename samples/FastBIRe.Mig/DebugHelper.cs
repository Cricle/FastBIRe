using FastBIRe;

internal static class DebugHelper
{
    public static void OnExecuterScriptStated(object sender, ScriptExecuteEventArgs e)
    {
        if (e.TryToKnowString(out var str))
        {
            Console.WriteLine(str);
        }

    }

}
