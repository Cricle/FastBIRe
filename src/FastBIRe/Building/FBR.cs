using FastBIRe.Functions;
using System.Reflection;

namespace FastBIRe.Building
{
    public static class FBR
    {
        internal static readonly MethodBase InvokeMethod = typeof(FBR).GetMethod(nameof(Invoke), BindingFlags.Static | BindingFlags.Public)!;

        public static object Invoke(SQLFunctions function,params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
