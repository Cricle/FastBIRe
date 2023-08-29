namespace FastBIRe.Project
{
    public readonly struct MigrateToSqlRestul
    {
        public readonly List<string> Sqls;

        public readonly MigrationService Serivce;

        public MigrateToSqlRestul(List<string> sqls, MigrationService serivce)
        {
            Sqls = sqls;
            Serivce = serivce;
        }

        public Task<int> ExecuteAsync(CancellationToken token = default)
        {
            if (Sqls == null || Sqls.Count == 0)
            {
                return Task.FromResult(0);
            }
            return Serivce.ExecuteNonQueryAsync(Sqls, token);
        }
        public override string ToString()
        {
            if (Sqls == null)
            {
                return "No sql";
            }
            return string.Join("\n", Sqls);
        }
    }

}
