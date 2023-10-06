namespace FastBIRe.Timescale
{
    public class TimescaleManager : ITimescaleManager
    {
        public TimescaleManager(string table, string timeColumn, IScriptExecuter scriptExecuter)
            : this(table, timeColumn, TimescaleHelper.Default, scriptExecuter)
        {
        }
        public TimescaleManager(string table, string timeColumn, TimescaleHelper helper, IScriptExecuter scriptExecuter)
        {
            Table = table;
            TimeColumn = timeColumn;
            Helper = helper;
            ScriptExecuter = scriptExecuter;
        }

        public string Table { get; }

        public string TimeColumn { get; }

        public string? ChuckTimeInterval { get; set; }

        public int CommandTimeout { get; }

        public TimescaleHelper Helper { get; }

        public IScriptExecuter ScriptExecuter { get; }

        public async Task<bool> CreateHypertableAsync(bool quto)
        {
            var table = quto ? $"'{Table}'" : Table;
            var timeColumn = quto ? $"'{TimeColumn}'" : TimeColumn;
            var sql = "SELECT " + Helper.CreateHypertable(table, timeColumn,
                if_not_exists: true,
                chunk_time_interval: ChuckTimeInterval);
            var res=await ScriptExecuter.ExecuteAsync(sql);
            return true;
        }

        public async Task<bool> IsHypertableAsync()
        {
            var sql = TimescaleViews.GetHypertable(Table, true);
            var isHyper = false;
            var res = await ScriptExecuter.ReadAsync(sql, (o, e) =>
            {
                if (e.Reader.Read())
                {
                    isHyper = !e.Reader.IsDBNull(0);
                }
            });
            return isHyper;
        }
    }
}
