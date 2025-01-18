namespace FastBIRe.DuckDB
{
    public partial class Statement
    {
        public static string OverEmpty(string expression)
        {
            return Over(expression, string.Empty);
        }
        public static string Over(string expression, string over)
        {
            return $"{expression} OVER ({over})";
        }
        public static string OverOrderBy(string expression, string orderBy)
        {
            return Over(expression, $"ORDER BY {orderBy}");
        }
        public static string OverPartitionBy(string expression, string partitionBy)
        {
            return Over(expression, $"PARTITION BY {partitionBy}");
        }
        public static string OverPartitionByOrderBy(string expression, string partitionBy, string orderBy)
        {
            return Over(expression, $"PARTITION BY {partitionBy} ORDER BY {orderBy}");
        }
        public static string Frame(string up, string down)
        {
            return $"ORWS BETWEEN {up} PRECEDING AND {down} FOLLOWING";
        }
    }
}
