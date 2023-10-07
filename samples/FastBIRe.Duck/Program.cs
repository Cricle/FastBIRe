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
        static void Main(string[] args)
        {
            var builder = new DuckDBConnectionStringBuilder();
            builder.DataSource = "q.duckdb";
            var duckConn = new DuckDBConnection(builder.ConnectionString);
            duckConn.Open();
            //var table = new DatabaseTable
            //{
            //    Name = "test"
            //};
            //table.AddColumn("id").SetTypeDefault(SqlType.DuckdDB, DbType.Int32).AddIdentity();
            //table.AddColumn("a1").SetTypeDefault(SqlType.DuckdDB, DbType.DateTime).AddIndex("IDX_11");
            //table.AddColumn("a2").SetTypeDefault(SqlType.DuckdDB, DbType.Double);
            //table.AddColumn("a3").SetTypeDefault(SqlType.DuckdDB, DbType.Decimal);
            //table.AddColumn("a4").SetTypeDefault(SqlType.DuckdDB, DbType.Boolean);
            //table.PrimaryKey = new DatabaseConstraint { Columns = { table.Columns[0].Name } };
            //var ddl = new DdlGeneratorFactory(SqlType.DuckdDB).TableGenerator(table).Write();
            //Console.WriteLine(ddl);
            var reader = new DatabaseReader(duckConn);
            var tb = reader.Table("test");
            var cptb = reader.Table("test");
            cptb.Columns[1].Id = 1;
            tb.Columns[1].Id = 1;

            cptb.Columns[1].Name = "qweqewdawda";
            var ddl = CompareSchemas.FromTable(builder.DataSource, SqlType.DuckdDB, tb, cptb).Execute();
            Console.WriteLine(ddl);
        }
    }
}