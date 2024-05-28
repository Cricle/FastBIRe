using FastBIRe.Cdc.Checkpoints;
using System.Collections.Generic;

namespace FastBIRe.Cdc
{
    public class CdcDataExpandBuilder
    {
        public CdcDataExpandBuilder()
        {
            Inserts = new CdcDataRowBuilder<ICdcDataRow>();
            Updates = new CdcDataRowBuilder<ICdcUpdateRow>();
            Deletes = new CdcDataRowBuilder<ICdcDataRow>();
        }

        public CdcDataRowBuilder<ICdcDataRow> Inserts { get; }

        public CdcDataRowBuilder<ICdcUpdateRow> Updates { get; }

        public CdcDataRowBuilder<ICdcDataRow> Deletes { get; }
    }
    public class CdcDataRowBuilder<TRow>
    {
        public CdcDataRowBuilder()
        {
            Rows = new List<TRow>();
        }

        public IList<TRow> Rows { get; }

        public bool HasRows => Rows.Count > 0;

        public ICheckpoint? Checkpoint { get; set; }

        public void AddAndSet(TRow row, ICheckpoint? checkpoint)
        {
            Rows.Add(row);
            Checkpoint = checkpoint;
        }
    }
}
