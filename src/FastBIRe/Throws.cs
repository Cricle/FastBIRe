namespace FastBIRe
{
    internal static class Throws
    {
        public static void ThrowTableNotFound(string tableName)
        {
            throw new ArgumentException($"Table {tableName} not found");
        }
        public static void ThrowFieldNotFound(string? fieldName, string tableName)
        {
            throw new ArgumentException($"Field {fieldName} not found on table {tableName}");
        }
    }
}
