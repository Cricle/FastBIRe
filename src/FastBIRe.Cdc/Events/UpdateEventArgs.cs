using FastBIRe.Cdc.Checkpoints;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FastBIRe.Cdc.Events
{
    public class UpdateEventArgs : OperatorCdcEventArgs, IEnumerable<IEnumerable<object?>>
    {
        public UpdateEventArgs(object? rawData, object tableId, ITableMapInfo? tableInfo, IList<ICdcUpdateRow> rows, ICheckpoint? checkpoint)
            : base(rawData, tableId, tableInfo, checkpoint)
        {
            Rows = rows;
        }
        public IList<ICdcUpdateRow> Rows { get; }

        public IEnumerator<IEnumerable<object?>> GetEnumerator()
        {
            return Rows.Select(x => x.AfterRow.AsEnumerable()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
