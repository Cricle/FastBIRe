using DatabaseSchemaReader.DataSchema;
using System.Data.Common;

namespace FastBIRe
{
    public static class IndexByteLenHelper
    {
        public static async Task<int> GetIndexByteLenAsync(DbConnection connection, FSqlType sqlType, int timeOut = 60 * 5, CancellationToken token = default)
        {
            string? sql;
            switch (sqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    sql = @"SELECT
    CASE WHEN CONVERT(NVARCHAR(128),SERVERPROPERTY('Edition')) LIKE '%Enterprise Edition%'
        THEN 900
        WHEN CONVERT(NVARCHAR(128),SERVERPROPERTY('Edition')) LIKE '%Developer Edition%'
        THEN 900
        WHEN CONVERT(NVARCHAR(128),SERVERPROPERTY('Edition')) LIKE '%Standard Edition%'
        THEN 700
        WHEN CONVERT(NVARCHAR(128),SERVERPROPERTY('Edition')) LIKE '%Web Edition%'
        THEN 700
        ELSE 400
    END AS max_index_length;";
                    break;
                case FSqlType.MySql:
                    //768~3072
                    sql = "SHOW VARIABLES LIKE 'innodb_large_prefix';";
                    break;
                case FSqlType.SQLite:
                    sql = "PRAGMA page_size;";
                    break;
                case FSqlType.PostgreSql:
                    sql = "SELECT current_setting('block_size')::int * 32767;";
                    break;
                case FSqlType.Oracle:
                case FSqlType.Db2:
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = timeOut;
                token.ThrowIfCancellationRequested();
                var scan = await command.ExecuteScalarAsync(token);
                switch (sqlType)
                {
                    case FSqlType.SqlServer:
                    case FSqlType.SqlServerCe:
                    case FSqlType.SQLite:
                    case FSqlType.PostgreSql:
                        return Convert.ToInt32(scan);
                    case FSqlType.MySql:
                        if (scan == null)
                        {
                            return 768;
                        }
                        return 3072;
                    default:
                        break;
                }
            }
            return 0;
        }
    }
}
