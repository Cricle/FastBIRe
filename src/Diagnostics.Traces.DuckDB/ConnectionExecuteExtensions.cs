using System.Data.Common;

namespace Diagnostics.Traces.DuckDB
{
    internal static class ConnectionExecuteExtensions
    {
        public static int ExecuteNoQuery(this DbConnection connection, string sql)
        {
            using (var comm = connection.CreateCommand())
            {
                comm.CommandText = sql;
                return comm.ExecuteNonQuery();
            }
        }
    }
}
