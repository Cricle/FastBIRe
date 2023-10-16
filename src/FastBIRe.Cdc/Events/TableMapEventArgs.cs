namespace FastBIRe.Cdc.Events
{
    public class TableMapEventArgs : OperatorCdcEventArgs
    {
        public TableMapEventArgs(object? rawData, object tableId, ITableMapInfo? tableInfo) 
            : base(rawData,tableId,tableInfo)
        {
        }

    }
}
