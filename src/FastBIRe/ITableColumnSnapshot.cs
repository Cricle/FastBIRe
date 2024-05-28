namespace FastBIRe
{
    public interface ITableColumnSnapshot
    {
        string Name { get; }

        string WrapName { get; }

        int Index { get; }
    }
}
