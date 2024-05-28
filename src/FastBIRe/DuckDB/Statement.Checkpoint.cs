namespace FastBIRe.DuckDB
{
    public partial class Statement
    {
        public static string CheckPoint => "CHECKPOINT;";
        public static string ForceCheckPoint => "FORCE CHECKPOINT;";

        public static string CheckPointDatabase(string database)
        {
            return $"CHECKPOINT {database};";
        }
    }
}
