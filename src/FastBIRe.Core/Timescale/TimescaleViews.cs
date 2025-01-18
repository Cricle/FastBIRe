namespace FastBIRe.Timescale
{
    public static class TimescaleViews
    {
        public const string Chucks = "timescaledb_information.chunks";
        public const string ContinuousAggregates = "timescaledb_information.continuous_aggregates";
        public const string CompressionSettings = "timescaledb_information.compression_settings";
        public const string DataNodes = "timescaledb_information.data_nodes";
        public const string Dimensions = "timescaledb_information.dimensions";
        public const string Hypertables = "timescaledb_information.hypertables";
        public const string Jobs = "timescaledb_information.jobs";
        public const string JobStats = "timescaledb_information.job_stats";
        public const string JobErrors = "timescaledb_information.job_errors";
        public const string Policies = "timescaledb_information.policies";

        public static string GetHypertable(string hypertable, bool hasMode)
        {
            return $"SELECT {(hasMode ? "1" : "*")} FROM {Hypertables} WHERE hypertable_name = '{hypertable}'";
        }
        public static string GetContinuousAggregate(string name, bool hasMode)
        {
            return $"SELECT {(hasMode ? "1" : "*")} FROM {ContinuousAggregates} WHERE view_name = '{name}'";
        }
    }
}
