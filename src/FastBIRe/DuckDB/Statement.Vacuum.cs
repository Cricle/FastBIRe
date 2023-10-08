namespace FastBIRe.DuckDB
{
    public static partial class Statement
    {
        public static string Vacuum => "VACUUM;";
        public static string VacuumAnalyze => "VACUUM ANALYZE;";

        public static string VacuumAnalyzeColumn(string columnPath)
        {
            return $"VACUUM ANALYZE {columnPath};";
        }
    }
}
