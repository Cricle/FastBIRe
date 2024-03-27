using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using DuckDB.NET.Data;
using System.Data;

namespace FastBIRe.Duck
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new DuckDBConnectionStringBuilder();
            builder.DataSource = "q.duckdb";
            var duckConn = new DuckDBConnection(builder.ConnectionString);
            duckConn.Open();
            var executer = new DefaultScriptExecuter(duckConn);
            var reader = new DatabaseReader(duckConn);
            var hasTable = reader.TableExists("test");
            if (!hasTable)
            {
                var table = new DatabaseTable
                {
                    Name = "test"
                };
                table.AddColumn("id").SetTypeDefault(SqlType.DuckDB, DbType.Int32).AddIdentity();
                table.AddColumn("a1").SetTypeDefault(SqlType.DuckDB, DbType.DateTime).AddIndex("IDX_11");
                table.AddColumn("a2").SetTypeDefault(SqlType.DuckDB, DbType.Double);
                table.AddColumn("a3").SetTypeDefault(SqlType.DuckDB, DbType.Decimal);
                table.AddColumn("a4").SetTypeDefault(SqlType.DuckDB, DbType.Boolean);
                table.PrimaryKey = new DatabaseConstraint { Columns = { table.Columns[0].Name } };
                var ddl = new DdlGeneratorFactory(SqlType.DuckDB).TableGenerator(table).Write();
                Console.WriteLine(ddl);
                await executer.ExecuteAsync(ddl);
            }
            var tb = reader.Table("test");
            var cptb = reader.Table("test");
            cptb.Columns[1].Id = 1;
            tb.Columns[1].Id = 1;

            cptb.Columns[1].Name = "qweqewdawda";
            var script = CompareSchemas.FromTable(builder.DataSource, SqlType.DuckDB, tb, cptb).Execute();
            Console.WriteLine(script);
            await executer.ExecuteAsync(script);
        }
    }
}