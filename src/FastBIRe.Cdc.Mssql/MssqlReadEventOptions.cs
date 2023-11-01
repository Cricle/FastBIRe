using System.Numerics;

namespace FastBIRe.Cdc.Mssql
{
    public class MssqlReadEventOptions
    {
        public MssqlReadEventOptions(MssqlCdcListener listener, MssqlTableMapInfo table)
        {
            Listener = listener;
            Table = table;
        }

        public MssqlCdcListener Listener { get; }

        public MssqlTableMapInfo Table { get; }

        public MssqlLsn Lsn { get; set; }
    }
}
