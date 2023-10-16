using System.Collections.Generic;

namespace FastBIRe.Cdc.Events
{
    public class InsertEventArgs : OperatorCdcEventArgs
    {
        public InsertEventArgs(object? rawData, object tableId, ITableMapInfo? tableInfo, IList<ICdcDataRow> rows) 
            : base(rawData,tableId,tableInfo)
        {
            Rows = rows;
        }

        public IList<ICdcDataRow> Rows { get; set; }
    }
}
