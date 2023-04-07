namespace FastBIRe
{
    public static class PostgresqlHelper
    {
        public static string GetFunName(string name)
        {
            return "fun_" + name.Replace('-', '_');
        }
    }
}
