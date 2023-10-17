using FastBIRe.Cdc.Checkpoints;
using System.Collections.Generic;

namespace FastBIRe.Cdc.Events
{
    public class DeleteEventArgs : OperatorCdcEventArgs
    {
        public DeleteEventArgs(object? rawData, object tableId, ITableMapInfo? tableInfo, IList<ICdcDataRow> rows,ICheckpoint? checkpoint)
            : base(rawData, tableId, tableInfo,checkpoint)
        {
            Rows = rows;
        }

        public IList<ICdcDataRow> Rows { get; set; }
    }
}
