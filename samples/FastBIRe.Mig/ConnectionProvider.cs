using DatabaseSchemaReader.DataSchema;
using DuckDB.NET.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using System.Data.Common;

namespace rsa
{
    internal static class ConnectionProvider
    {
        //public static async Task EnsureDatabaseCreatedAsync(SqlType type, string database, bool mariadb = false)
        //{
        //    using (var createMig = GetDbMigration(type, null, mariadb))
        //    {
        //        await createMig.EnsureDatabaseCreatedAsync(database);
        //    }
        //}
        public static DbConnection GetDbMigration(SqlType type, string database, bool mariadb = false)
        {
            DbConnection conn = null;
            switch (type)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    conn = new SqlConnection($"Server=192.168.1.101;Uid=sa;Pwd=Syc123456.;Connection Timeout=2000;TrustServerCertificate=true{(string.IsNullOrEmpty(database) ? string.Empty : $";Database={database};")}");
                    break;
                case SqlType.MySql:
                    if (mariadb)
                    {
                        conn = new MySqlConnection($"Server=192.168.1.95;Port=3307;Uid=root;Pwd=syc123;Connection Timeout=2000;Character Set=utf8{(string.IsNullOrEmpty(database) ? string.Empty : $";Database={database};")}");
                    }
                    else
                    {
                        conn = new MySqlConnection($"Server=127.0.0.1;Port=3306;Uid=root;Pwd=355343;Connection Timeout=2000;Character Set=utf8{(string.IsNullOrEmpty(database) ? string.Empty : $";Database={database};")}");
                    }
                    break;
                case SqlType.SQLite:
                    conn = new SqliteConnection($"{(string.IsNullOrEmpty(database) ? string.Empty : $"Data Source={database}.db;")}");
                    break;
                case SqlType.PostgreSql:
                    conn = new NpgsqlConnection($"host=192.168.1.101;port=5432;username=postgres;password=Syc123456.;{(string.IsNullOrEmpty(database) ? string.Empty : $";Database={database};")}");
                    break;
                case SqlType.DuckDB:
                    var builder = new DuckDBConnectionStringBuilder();
                    if (database == null)
                    {
                        builder.DataSource = ":memory:";
                    }
                    else
                    {
                        builder.DataSource = database;
                    }
                    conn = new DuckDBConnection(builder.ConnectionString);
                    break;
                default:
                    break;
            }
            conn!.Open();
            return conn;
        }

    }
}