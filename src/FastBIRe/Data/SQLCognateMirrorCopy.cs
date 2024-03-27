namespace FastBIRe.Data
{
    public class SQLCognateMirrorCopy : IMirrorCopy<SQLMirrorCopyResult>
    {
        public SQLCognateMirrorCopy(IScriptExecuter scriptExecuter, string sourceSql, string targetNamed)
        {
            ScriptExecuter = scriptExecuter ?? throw new ArgumentNullException(nameof(scriptExecuter));
            SourceSql = sourceSql ?? throw new ArgumentNullException(nameof(sourceSql));
            TargetNamed = targetNamed ?? throw new ArgumentNullException(nameof(targetNamed));
        }

        public IScriptExecuter ScriptExecuter { get; }

        public string SourceSql { get; }

        public string TargetNamed { get; }

        public int CommandTimeout { get; set; } = 60 * 5;

        protected virtual string GetInsertQuery()
        {
            return $"INSERT INTO {TargetNamed} {SourceSql}";
        }

        public async Task<IList<SQLMirrorCopyResult>> CopyAsync(CancellationToken token)
        {
            var query = GetInsertQuery();
            var result = await ScriptExecuter.ExecuteAsync(query, token: token);
            return new[]
            {
                new SQLMirrorCopyResult(result,query)
            };
        }
    }
}
