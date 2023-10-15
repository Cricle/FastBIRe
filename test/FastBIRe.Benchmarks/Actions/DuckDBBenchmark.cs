using DuckDB.NET.Data;

namespace FastBIRe.Benchmarks.Actions
{
    public abstract class DuckDBBenchmark
    {
        public DuckDBConnection Connection { get; }

        public DuckDBBenchmark()
        {
            Connection= new DuckDBConnection(DuckDBConnectionStringBuilder.InMemoryConnectionString);
            Connection.Open();
        }

        public int Script(string sql)
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = sql;
                return comm.ExecuteNonQuery();
            }
        }
        public async Task<int> ScriptAsync(string sql)
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = sql;
                return await comm.ExecuteNonQueryAsync();
            }
        }
    }
}
