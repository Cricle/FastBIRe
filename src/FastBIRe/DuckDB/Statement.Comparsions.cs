namespace FastBIRe.DuckDB
{
    public partial class Statement
    {
        public static string Euquals(string left, string right)
        {
            return $"{left} IS NOT DISTINCT FROM {right}";
        }
        public static string NotEuquals(string left, string right)
        {
            return $"{left} IS DISTINCT FROM {right}";
        }
    }
}
