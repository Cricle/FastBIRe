namespace Diagnostics.Traces.Status
{
    public static class StatusScopeExtensions
    {
        public static bool Log(this IStatusScope scope, string format, object? arg0)
        {
            return scope.Log(string.Format(format, arg0));
        }
        public static bool Log(this IStatusScope scope, string format, object? arg0, object? arg1)
        {
            return scope.Log(string.Format(format, arg0, arg1));
        }
        public static bool Log(this IStatusScope scope, string format, object? arg0, object? arg1,object? arg2)
        {
            return scope.Log(string.Format(format, arg0, arg1, arg2));
        }
        public static bool Log(this IStatusScope scope, string format, params object[] args)
        {
            return scope.Log(string.Format(format, args));
        }
        public static Task<bool> LogAsync(this IStatusScope scope, string format, params object[] args)
        {
            return scope.LogAsync(string.Format(format, args));
        }

        public static bool Set(this IStatusScope scope, string format, object? arg0)
        {
            return scope.Set(string.Format(format, arg0));
        }
        public static bool Set(this IStatusScope scope, string format, object? arg0, object? arg1)
        {
            return scope.Set(string.Format(format, arg0, arg1));
        }
        public static bool Set(this IStatusScope scope, string format, object? arg0, object? arg1, object? arg2)
        {
            return scope.Set(string.Format(format, arg0, arg1, arg2));
        }
        public static bool Set(this IStatusScope scope, string format, params object[] args)
        {
            return scope.Set(string.Format(format, args));
        }
        public static Task<bool> SetAsync(this IStatusScope scope, string format, params object[] args)
        {
            return scope.SetAsync(string.Format(format, args));
        }
    }
}