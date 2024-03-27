namespace FastBIRe.Cdc
{
    public interface ICdcUpdateRow
    {
        ICdcDataRow? BeforeRow { get; }

        ICdcDataRow AfterRow { get; }
    }
}
