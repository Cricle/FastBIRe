using Diagnostics.Traces.Stores;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBResultInitializer : IUndefinedResultInitializer<DuckDBDatabaseCreatedResult>
    {
        private const string InitSql = @"

";

        public void InitializeResult(DuckDBDatabaseCreatedResult result)
        {

        }
    }
}
