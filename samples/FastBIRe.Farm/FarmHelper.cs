using DatabaseSchemaReader;
using DuckDB.NET.Data;
using FastBIRe.AP.DuckDB;
using MySqlConnector;
using Npgsql;

namespace FastBIRe.Farm
{
    internal static class FarmHelper
    {
        public static FarmManager CreateFarm(string tableName,bool listen)
        {
            var duck = new DuckDBConnection("Data source=a.db");
            //var mysql = new MySqlConnection("Server=192.168.1.101;Port=3306;Uid=root;Pwd=Syc123456.;Connection Timeout=2000;Character Set=utf8;Database=test-2");
            var mysql = new NpgsqlConnection("host=192.168.1.101;port=5432;username=postgres;password=Syc123456.;database=test-2");

            duck.Open();
            mysql.Open();

            var reader = new DatabaseReader(mysql) { Owner = mysql.Database };
            var table = reader.Table(tableName);
            var mysqlExecuter = new DefaultScriptExecuter(mysql);
            var duckExecuter = new DefaultScriptExecuter(duck);
            if (listen)
            {
                mysqlExecuter.ScriptStated += DebugHelper.OnExecuterScriptStated!;
                duckExecuter.ScriptStated += DebugHelper.OnExecuterScriptStated!;
            }
            var sourceHouse = new FarmWarehouse(mysqlExecuter);
            var destHouse = new DuckFarmWarehouse(duckExecuter);
            return new FarmManager(table, sourceHouse, destHouse);
        }

    }
}
