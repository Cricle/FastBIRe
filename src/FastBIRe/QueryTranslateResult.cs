namespace FastBIRe
{
    public readonly struct QueryTranslateResult : IQueryTranslateResult
    {
        private static readonly IReadOnlyDictionary<string, object> EmptyArgs = new Dictionary<string, object>(0);

        public QueryTranslateResult(string queryString)
        {
            QueryString = queryString;
            Args = EmptyArgs;
        }
        public QueryTranslateResult(string queryString, IReadOnlyDictionary<string, object> args)
        {
            QueryString = queryString;
            Args = args;
        }

        public string QueryString { get; }

        public IReadOnlyDictionary<string, object> Args { get; }
    }
}
