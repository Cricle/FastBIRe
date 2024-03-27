using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Cdc.Mssql
{
    public class TriggerReadEventOptions
    {
        public TriggerReadEventOptions(TriggerCdcListener listener, TriggerTableMapInfo table, SqlType sqlType, int batchSize)
        {
            Listener = listener;
            Table = table;
            SqlType = sqlType;
            BatchSize = batchSize;
        }

        public TriggerCdcListener Listener { get; }

        public TriggerTableMapInfo Table { get; }

        public SqlType SqlType { get; }

        public int BatchSize { get; }
    }
}
