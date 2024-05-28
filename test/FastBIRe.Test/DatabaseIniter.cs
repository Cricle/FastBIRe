using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Data.Common;

namespace FastBIRe.Test
{
    public class DatabaseIniter
    {
        public static readonly DatabaseIniter Instance = new DatabaseIniter();

        private string mysql;
        private string sqlserver;
        private string sqlite;
        private string postgresql;

        public DatabaseIniter()
        {
            var json = File.ReadAllText("connects.json");
            var jobj = JObject.Parse(json);
            mysql = jobj["MySql"]!.ToString();
            sqlserver = jobj["SqlServer"]!.ToString();
            sqlite = jobj["Sqlite"]!.ToString();
            postgresql = jobj["Postgresql"]!.ToString();
        }

        public DbConnection Get(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return SqlServer();
                case SqlType.MySql:
                    return MySql();
                case SqlType.SQLite:
                    return Sqlite();
                case SqlType.PostgreSql:
                    return PostgreSql();
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }

        public DbConnection Sqlite()
        {
            var conn = new SqliteConnection(sqlite);
            conn.Open();
            connections.Add(conn);
            return conn;
        }
        public DbConnection MySql()
        {
            var conn = new MySqlConnection(mysql);
            conn.Open();
            connections.Add(conn);
            return conn;
        }
        public DbConnection PostgreSql()
        {
            var conn = new NpgsqlConnection(postgresql);
            conn.Open();
            connections.Add(conn);
            return conn;
        }
        public DbConnection SqlServer()
        {
            var conn = new SqlConnection(sqlserver);
            conn.Open();
            connections.Add(conn);
            return conn;
        }

        public async Task ReturnAsync(DbConnection connection, bool drop)
        {
            if (drop)
            {
                await CleanupConnectionAsync(connection);
            }
            else
            {
                connection.Dispose();
                connections.Remove(connection);
            }
        }

        private readonly List<DbConnection> connections = new List<DbConnection>();

        public async Task CleanupConnectionAsync(DbConnection item)
        {
            if (item is SqliteConnection sqliteConnection)
            {
                var file = sqliteConnection.DataSource;
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            else if (item is MySqlConnection mySqlConnection)
            {
                await mySqlConnection.ExecuteAsync($"Drop database `{mySqlConnection.Database}`");
            }
            else if (item is NpgsqlConnection npgsqlConnection)
            {
                await npgsqlConnection.ExecuteAsync($"Drop database \"{npgsqlConnection.Database}\"");
            }
            else if (item is SqlConnection sqlConnection)
            {
                await sqlConnection.ExecuteAsync($"Drop database [{sqlConnection.Database}]");
            }
            item.Dispose();
            connections.Remove(item);
        }
        [TestCleanup]
        public async Task ClenupAsync()
        {
            var copy = connections.ToList();
            foreach (var item in copy)
            {
                await CleanupConnectionAsync(item);
            }
            connections.Clear();
        }

    }
}
