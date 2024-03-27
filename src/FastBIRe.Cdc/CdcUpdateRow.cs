namespace FastBIRe.Cdc
{
    public class CdcUpdateRow : ICdcUpdateRow
    {
        public CdcUpdateRow(ICdcDataRow? beforeRow, ICdcDataRow afterRow)
        {
            BeforeRow = beforeRow;
            AfterRow = afterRow;
        }

        public ICdcDataRow? BeforeRow { get; }

        public ICdcDataRow AfterRow { get; }
    }
}
