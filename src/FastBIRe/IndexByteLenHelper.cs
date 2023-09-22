using Ao.Stock.Mirror;
using DatabaseSchemaReader.DataSchema;
using System.Data.Common;

namespace FastBIRe
{
    public static class IndexByteLenHelper
    {
        public static async Task<int> GetIndexByteLenAsync(DbConnection connection, SqlType sqlType, int timeOut = 60 * 5, CancellationToken token = default)
        {
            string? sql;
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
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
                case SqlType.MySql:
                    //768~3072
                    sql = "SHOW VARIABLES LIKE 'innodb_large_prefix';";
                    break;
                case SqlType.SQLite:
                    sql = "PRAGMA page_size;";
                    break;
                case SqlType.PostgreSql:
                    sql = "SELECT current_setting('block_size')::int * 32767;";
                    break;
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
            using (var command = connection.CreateCommand(sql))
            {
                command.CommandTimeout = timeOut;
                token.ThrowIfCancellationRequested();
                var scan = await command.ExecuteScalarAsync(token);
                switch (sqlType)
                {
                    case SqlType.SqlServer:
                    case SqlType.SqlServerCe:
                    case SqlType.SQLite:
                    case SqlType.PostgreSql:
                        return Convert.ToInt32(scan);
                    case SqlType.MySql:
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
