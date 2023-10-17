using FastBIRe.Cdc.Checkpoints;
using System.Collections.Generic;

namespace FastBIRe.Cdc.Events
{
    public class UpdateEventArgs : OperatorCdcEventArgs
    {
        public UpdateEventArgs(object? rawData, object tableId, ITableMapInfo? tableInfo, IList<ICdcUpdateRow> rows, ICheckpoint? checkpoint)
            : base(rawData, tableId, tableInfo,checkpoint)
        {
            Rows = rows;
        }
        public IList<ICdcUpdateRow> Rows { get; }

    }
}
