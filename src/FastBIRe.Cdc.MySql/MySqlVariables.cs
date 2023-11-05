namespace FastBIRe.Cdc.MySql
{
    public class MySqlVariables : DbVariables
    {
        public const string LogBinKey = "log_bin";

        public const string MaxBinlogSizeKey = "max_binlog_size";

        public const string GtidModeKey = "GTID_MODE";

        public const string EnfprceGtodConsistencyKey = "ENFORCE_GTID_CONSISTENCY";

        public bool LogBin => GetAndEquals(LogBinKey, "on");

        public long MaxBinlogSize => GetAndCase<long>(MaxBinlogSizeKey);

        public bool GtidMode => GetAndEquals(GtidModeKey, "on");

        public bool EnfprceGtodConsistency => GetAndEquals(EnfprceGtodConsistencyKey, "on");
    }
}
