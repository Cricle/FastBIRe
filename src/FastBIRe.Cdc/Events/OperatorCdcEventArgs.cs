using FastBIRe.Cdc.Checkpoints;

namespace FastBIRe.Cdc.Events
{
    public class OperatorCdcEventArgs : CdcEventArgs
    {
        public OperatorCdcEventArgs(object? rawData, object tableId, ITableMapInfo? tableInfo, ICheckpoint? checkpoint)
            : base(rawData, checkpoint)
        {
            TableId = tableId;
            TableInfo = tableInfo;
        }
        public object TableId { get; }

        public ITableMapInfo? TableInfo { get; }
    }
}
