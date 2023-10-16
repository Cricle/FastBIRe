namespace FastBIRe.Cdc.Events
{
    public class OperatorCdcEventArgs : CdcEventArgs
    {
        public OperatorCdcEventArgs(object? rawData, object tableId, ITableMapInfo? tableInfo)
            : base(rawData)
        {
            TableId = tableId;
            TableInfo = tableInfo;
        }
        public object TableId { get; }

        public ITableMapInfo? TableInfo { get; }
    }
}
