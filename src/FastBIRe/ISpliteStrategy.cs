namespace FastBIRe
{
    public interface ISpliteStrategy
    {
        string GetTable(IEnumerable<object> values, int offset);
    }
}
