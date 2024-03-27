namespace FastBIRe
{
    public interface IQueryTranslateResult
    {
        string QueryString { get; }

        IEnumerable<KeyValuePair<string, object?>> Args { get; }
    }
}
