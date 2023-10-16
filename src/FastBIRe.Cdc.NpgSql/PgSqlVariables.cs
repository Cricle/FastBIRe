namespace FastBIRe.Cdc.NpgSql
{
    public class PgSqlVariables : DbVariables
    {
        public const string WalLevelKey = "wal_level";

        public PgSqlWalLevel? WalLevel
        {
            get
            {
                var wl = GetOrDefault(WalLevelKey);
                if (string.IsNullOrEmpty(wl))
                {
                    return null;
                }
                if (string.Equals(wl,"minimal", StringComparison.OrdinalIgnoreCase))
                {
                    return PgSqlWalLevel.Minimal;
                }
                if (string.Equals(wl, "replica", StringComparison.OrdinalIgnoreCase))
                {
                    return PgSqlWalLevel.Replica;
                }
                if (string.Equals(wl, "logical", StringComparison.OrdinalIgnoreCase))
                {
                    return PgSqlWalLevel.Logical;
                }
                return null;
            }
        }
    }
}
