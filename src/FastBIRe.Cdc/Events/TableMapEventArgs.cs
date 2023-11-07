using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.Events
{
    public class TableMapEventArgs : OperatorCdcEventArgs
    {
        public TableMapEventArgs(object? rawData, object tableId, ITableMapInfo? tableInfo, ICheckpoint? checkpoint)
            : base(rawData, tableId, tableInfo, checkpoint)
        {
        }

    }
}
