namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public string AddRetentionPolicy(string relation,
            string drop_after,
            string? initial_start = null,
            string? timezone = null,
            bool? if_not_exists = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(initial_start))
                args.Add($"initial_start => {initial_start}");
            if (!string.IsNullOrEmpty(timezone))
                args.Add($"timezone => {timezone}");
            if (if_not_exists != null)
                args.Add($"if_not_exists => {BoolToString(if_not_exists)}");
            var sql = $"add_retention_policy({relation},{drop_after}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string RemoveRetentionPolicy(string relation)
        {
            return $"remove_retention_policy({relation})";
        }
    }
}
