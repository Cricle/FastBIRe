namespace FastBIRe.Cdc.MySql
{
    public class MySqlCdcLog : CdcLog
    {
        public MySqlCdcLog(string name, ulong? length, string? gtid) : base(name, length)
        {
            Gtid = gtid;
        }

        public string? Gtid { get; }
    }
}
