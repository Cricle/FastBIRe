using System.Data.Common;

namespace FastBIRe.Timescale
{
    public interface ITimescaleManager
    {
        string Table { get; }

        string TimeColumn { get; }

        string? ChuckTimeInterval { get; }

        string? Interval { get; }

        int CommandTimeout { get; }

        Task<bool> IsHypertableAsync();

        Task<bool> CreateHypertableAsync();
    }
    public class TimescaleManager : ITimescaleManager
    {
        public TimescaleManager(string table, string timeColumn, DbConnection connection)
            :this(table,timeColumn,TimescaleHelper.Default,connection)
        {
        }
        public TimescaleManager(string table, string timeColumn, TimescaleHelper helper, DbConnection connection)
        {
            Table = table;
            TimeColumn = timeColumn;
            Helper = helper;
            Connection = connection;
        }

        public string Table { get; }

        public string TimeColumn { get; }

        public string? ChuckTimeInterval { get; }

        public string? Interval { get; }

        public int CommandTimeout { get; }

        public TimescaleHelper Helper { get; }

        public DbConnection Connection { get; }

        public async Task<bool> CreateHypertableAsync()
        {
            var sql = Helper.CreateHypertable(Table, TimeColumn,
                if_not_exists: true,
                chunk_time_interval: ChuckTimeInterval);
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandTimeout = CommandTimeout;
                comm.CommandText = sql;
                var res = await comm.ExecuteNonQueryAsync();
                return true;
            }
        }

        public async Task<bool> IsHypertableAsync()
        {
            var sql = TimescaleViews.GetHypertable(Table, true);
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandTimeout = CommandTimeout;
                comm.CommandText = sql;
                var res = await comm.ExecuteScalarAsync();
                return res != null;
            }
        }
    }
}
