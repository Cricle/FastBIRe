namespace Diagnostics.Generator.Internal
{
    internal static class ConvertHelperExtensions
    {
        public static string ToBoolKeyword(this bool b)
        {
            return b ? "true" : "false";
        }
        public static string ObjectToCsharp(this object? obj)
        {
            switch (obj)
            {
                case null:
                    return "null";
                case int @int:
                    return @int.ToString();
                case double @double:
                    return @double.ToString();
                default:
                    return $"\"{obj}\"";
            }
        }
        public static string NullableToCSharp<T>(this T? b)
            where T : struct
        {
            if (b == null)
            {
                return "null";
            }
            if (b is float f)
            {
                return f.ToString() + "F";
            }
            if (b is decimal d)
            {
                return d.ToString() + "M";
            }
            return b.ToString();
        }
    }
}
