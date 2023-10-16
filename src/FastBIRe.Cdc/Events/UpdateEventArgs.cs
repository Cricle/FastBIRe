using System.Collections.Generic;

namespace FastBIRe.Cdc.Events
{
    public class UpdateEventArgs : OperatorCdcEventArgs
    {
        public UpdateEventArgs(object? rawData, object tableId, ITableMapInfo? tableInfo, IList<ICdcUpdateRow> rows)
            : base(rawData, tableId, tableInfo)
        {
            Rows = rows;
        }
        public IList<ICdcUpdateRow> Rows { get; }

    }
}
