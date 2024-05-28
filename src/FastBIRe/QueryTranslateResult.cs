using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public readonly struct QueryTranslateResult : IQueryTranslateResult
    {
        public static readonly IEnumerable<KeyValuePair<string, object?>> EmptyArgs = Enumerable.Empty<KeyValuePair<string, object?>>();

        public QueryTranslateResult(string queryString)
        {
            QueryString = queryString;
            Args = EmptyArgs;
        }

        public QueryTranslateResult(string queryString, IEnumerable<KeyValuePair<string, object?>> args)
        {
            QueryString = queryString;
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public string QueryString { get; }

        public IEnumerable<KeyValuePair<string, object?>> Args { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QueryTranslateResult Create(string queryString, IEnumerable<KeyValuePair<string, object?>>? args)
        {
            return new QueryTranslateResult(queryString, args ?? EmptyArgs);
        }
    }
}
