using FastBIRe.Cdc.Checkpoints;
using System.Collections;
using System.Collections.Generic;

namespace FastBIRe.Cdc.Events
{
    public class InsertEventArgs : OperatorCdcEventArgs, IEnumerable<IEnumerable<object?>>
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

        public IEnumerator<IEnumerable<object?>> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
