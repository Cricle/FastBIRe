namespace FastBIRe.Test
{
    [TestClass]
    public class TableHelperTest : DbTestBase
    {
        [TestMethod]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        public void CreateIndexFieldsWithoutDesc(SqlType sqlType)
        {
            var helper = new TableHelper(sqlType);
            var act = helper.CreateIndex("indexA", "table1", new string[] { "a1", "a2", "a3" });
            var exp = $"CREATE INDEX {Quto(sqlType, "indexA")} ON {Quto(sqlType, "table1")} ({Quto(sqlType, "a1")},{Quto(sqlType, "a2")},{Quto(sqlType, "a3")});";
            Assert.AreEqual(exp, act);
        }
        [TestMethod]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        public void CreateIndexFieldsWithFullDesc(SqlType sqlType)
        {
            var helper = new TableHelper(sqlType);
            var act = helper.CreateIndex("indexA", "table1", new string[] { "a1", "a2", "a3" }, new bool[] { true, false, false });
            var exp = $"CREATE INDEX {Quto(sqlType, "indexA")} ON {Quto(sqlType, "table1")} ({Quto(sqlType, "a1")} DESC,{Quto(sqlType, "a2")} ASC,{Quto(sqlType, "a3")} ASC);";
            Assert.AreEqual(exp, act);
        }
        [TestMethod]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        public void CreateIndexFieldsWithAnyDesc(SqlType sqlType)
        {
            var helper = new TableHelper(sqlType);
            var act = helper.CreateIndex("indexA", "table1", new string[] { "a1", "a2", "a3" }, new bool[] { true, false });
            var exp = $"CREATE INDEX {Quto(sqlType, "indexA")} ON {Quto(sqlType, "table1")} ({Quto(sqlType, "a1")} DESC,{Quto(sqlType, "a2")} ASC,{Quto(sqlType, "a3")});";
            Assert.AreEqual(exp, act);
        }
        [TestMethod]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        public void DropIndex(SqlType sqlType)
        {
            var helper = new TableHelper(sqlType);
            var act = helper.DropIndex("indexA", "table1");
            string exp = string.Empty;
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    exp = "DROP INDEX [table1].[indexA];";
                    break;
                case SqlType.MySql:
                    exp = "DROP INDEX `indexA` ON `table1`;";
                    break;
                case SqlType.SQLite:
                    exp = "DROP INDEX `indexA`;";
                    break;
                case SqlType.PostgreSql:
                    exp = "DROP INDEX \"indexA\";";
                    break;
                default:
                    break;
            }
            Assert.AreEqual(exp, act);
        }
    }
}
