namespace FastBIRe.Data
{
    public readonly record struct SQLMirrorCopyResult
    {
        private static readonly IReadOnlyDictionary<string, object> Empty = new Dictionary<string, object>(0);

        public SQLMirrorCopyResult(int affectRows, string sql)
        {
            AffectRows = affectRows;
            Sql = sql;
            Bindings = Empty;
        }
        public SQLMirrorCopyResult(int affectRows, string sql, IReadOnlyDictionary<string, object> bindings)
        {
            AffectRows = affectRows;
            Sql = sql;
            Bindings = bindings;
        }

        public int AffectRows { get; }

        public string Sql { get; }

        public IReadOnlyDictionary<string, object> Bindings { get; }
    }
}
