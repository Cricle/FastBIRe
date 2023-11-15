using FastBIRe.Cdc.Checkpoints;
using System.Collections.Generic;

namespace FastBIRe.Cdc.Events
{
    public class InsertEventArgs : OperatorCdcEventArgs
    {
        public InsertEventArgs(object? rawData, object tableId, ITableMapInfo? tableInfo, IList<ICdcDataRow> rows, ICheckpoint? checkpoint)
            : base(rawData, tableId, tableInfo, checkpoint)
        {
            Rows = rows;
        }

        public IList<ICdcDataRow> Rows { get; set; }

        public IEnumerable<IEnumerable<object?>> CreateObjectRange(int[]? mask)
        {
            foreach (var item in Rows)
            {
                if (mask == null)
                {
                    yield return item;
                }
                else
                {
                    yield return MaskRow(item, mask);
                }
            }
        }
        private IEnumerable<object> MaskRow(ICdcDataRow row, int[] mask)
        {
            for (int i = 0; i < mask.Length; i++)
            {
                yield return row[i];
            }
        }
    }
}
