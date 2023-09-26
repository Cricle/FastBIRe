namespace FastBIRe.Data
{
    public readonly record struct RowWriteResult<TKey>
    {
        public static readonly RowWriteResult<TKey> Empty = new RowWriteResult<TKey>();

        public RowWriteResult(IEnumerable<TKey>? keys, IEnumerable<IQueryTranslateResult>? translateResults, int affectRows)
        {
            Keys = keys;
            TranslateResults = translateResults;
            AffectRows = affectRows;
        }

        public IEnumerable<TKey>? Keys { get; }

        public IEnumerable<IQueryTranslateResult>? TranslateResults { get; }

        public int AffectRows { get; }
    }
}
