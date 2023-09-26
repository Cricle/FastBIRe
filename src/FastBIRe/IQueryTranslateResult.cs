namespace FastBIRe
{
    public interface IQueryTranslateResult
    {
        string QueryString { get; }

        IReadOnlyDictionary<string, object> Args { get; }
    }
}
