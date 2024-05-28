using System.Collections.Generic;

namespace FastBIRe.Cdc
{
    public class CdcDataRow : List<object?>, ICdcDataRow
    {
        public CdcDataRow()
        {
        }

        public CdcDataRow(IEnumerable<object?> collection) : base(collection)
        {
        }

        public CdcDataRow(int capacity) : base(capacity)
        {
        }
    }
}
