namespace FastBIRe
{
    public static class ScriptExecuterEventExtensions
    {
        public static void RegistScriptStated(this IScriptExecuter executer, EventHandler<ScriptExecuteEventArgs> handler)
        {
            if (executer is DefaultScriptExecuter scriptExecuter)
            {
                scriptExecuter.ScriptStated += handler;
                return;
            }
            throw new InvalidCastException($"Can't cast {executer.GetType()} to {typeof(DefaultScriptExecuter)}");
        }
        public static bool WhenDefault(this IScriptExecuter executer, Action<DefaultScriptExecuter> action)
        {
            if (executer is DefaultScriptExecuter scriptExecuter)
            {
                action(scriptExecuter);
                return true;
            }
            return false;
        }
        public static void UnRegistScriptStated(this IScriptExecuter executer, EventHandler<ScriptExecuteEventArgs> handler)
        {
            if (executer is DefaultScriptExecuter scriptExecuter)
            {
                scriptExecuter.ScriptStated -= handler;
                return;
            }
            throw new InvalidCastException($"Can't cast {executer.GetType()} to {typeof(DefaultScriptExecuter)}");
        }
    }
}
