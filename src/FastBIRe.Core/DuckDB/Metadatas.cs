namespace FastBIRe.DuckDB
{
    public static class Metadatas
    {
        public static string Settings => "duckdb_settings()";
        public static string Views => "duckdb_views()";
        public static string Columns => "duckdb_columns()";
        public static string Constraints => "duckdb_constraints()";
        public static string Database => "duckdb_databases()";
        public static string Dependencies => "duckdb_dependencies()";
        public static string Extensions => "duckdb_extensions()";
        public static string Functions => "duckdb_functions()";
    }
}
