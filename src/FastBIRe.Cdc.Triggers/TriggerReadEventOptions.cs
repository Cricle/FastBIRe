namespace FastBIRe.Cdc.Mssql
{
    public class TriggerReadEventOptions
    {
        public TriggerReadEventOptions(TriggerCdcListener listener, TriggerTableMapInfo table, FSqlType sqlType, int batchSize)
        {
            Listener = listener;
            Table = table;
            SqlType = sqlType;
            BatchSize = batchSize;
        }

        public TriggerCdcListener Listener { get; }

        public TriggerTableMapInfo Table { get; }

        public FSqlType SqlType { get; }

        public int BatchSize { get; }
    }
}
