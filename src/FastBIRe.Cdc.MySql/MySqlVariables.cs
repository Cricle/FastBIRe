namespace FastBIRe.Cdc.MySql
{
    public class MySqlVariables: DbVariables
    {
        public const string LogBinKey = "log_bin";

        public const string MaxBinlogSizeKey = "max_binlog_size";

        public bool LogBin => GetAndEquals(LogBinKey, "on");

        public long MaxBinlogSize => GetAndCase<long>(MaxBinlogSizeKey);
    }
}
