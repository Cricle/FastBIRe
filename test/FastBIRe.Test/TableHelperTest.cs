namespace FastBIRe.Test
{
    [TestClass]
    public class TableHelperTest : DbTestBase
    {
        [TestMethod]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        public void CreateIndexFieldsWithoutDesc(FSqlType sqlType)
        {
            var helper = new TableHelper(sqlType);
            var act = helper.CreateIndex("indexA", "table1", new string[] { "a1", "a2", "a3" });
            var exp = $"CREATE INDEX {Quto(sqlType, "indexA")} ON {Quto(sqlType, "table1")} ({Quto(sqlType, "a1")},{Quto(sqlType, "a2")},{Quto(sqlType, "a3")});";
            Assert.AreEqual(exp, act);
        }
        [TestMethod]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        public void CreateIndexFieldsWithFullDesc(FSqlType sqlType)
        {
            var helper = new TableHelper(sqlType);
            var act = helper.CreateIndex("indexA", "table1", new string[] { "a1", "a2", "a3" }, new bool[] { true, false, false });
            var exp = $"CREATE INDEX {Quto(sqlType, "indexA")} ON {Quto(sqlType, "table1")} ({Quto(sqlType, "a1")} DESC,{Quto(sqlType, "a2")} ASC,{Quto(sqlType, "a3")} ASC);";
            Assert.AreEqual(exp, act);
        }
        [TestMethod]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        public void CreateIndexFieldsWithAnyDesc(FSqlType sqlType)
        {
            var helper = new TableHelper(sqlType);
            var act = helper.CreateIndex("indexA", "table1", new string[] { "a1", "a2", "a3" }, new bool[] { true, false });
            var exp = $"CREATE INDEX {Quto(sqlType, "indexA")} ON {Quto(sqlType, "table1")} ({Quto(sqlType, "a1")} DESC,{Quto(sqlType, "a2")} ASC,{Quto(sqlType, "a3")});";
            Assert.AreEqual(exp, act);
        }
        [TestMethod]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        public void DropIndex(FSqlType sqlType)
        {
            var helper = new TableHelper(sqlType);
            var act = helper.DropIndex("indexA", "table1");
            string exp = string.Empty;
            switch (sqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    exp = "DROP INDEX [table1].[indexA];";
                    break;
                case FSqlType.MySql:
                    exp = "DROP INDEX `indexA` ON `table1`;";
                    break;
                case FSqlType.SQLite:
                    exp = "DROP INDEX `indexA`;";
                    break;
                case FSqlType.PostgreSql:
                    exp = "DROP INDEX \"indexA\";";
                    break;
                default:
                    break;
            }
            Assert.AreEqual(exp, act);
        }
    }
}
