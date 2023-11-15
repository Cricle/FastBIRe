namespace FastBIRe.Test
{
    [TestClass]
    public class OpimizeHelperTest
    {
        [TestMethod]
        public void MySql()
        {
            var act = new TableHelper(SqlType.MySql).Opimize("table");
            Assert.AreEqual("OPTIMIZE TABLE `table`;", act);
        }
        [TestMethod]
        public void SqlServer()
        {
            var act = new TableHelper(SqlType.SqlServer).Opimize("table");
            Assert.AreEqual("ALTER INDEX ALL ON [table] REBUILD;", act);
        }
        [TestMethod]
        public void Sqlite()
        {
            var act = new TableHelper(SqlType.SQLite).Opimize("table");
            Assert.AreEqual("VACUUM;", act);
        }
        [TestMethod]
        public void PostgreSql()
        {
            var act = new TableHelper(SqlType.PostgreSql).Opimize("table");
            Assert.AreEqual("VACUUM FULL \"table\";", act);
        }
        [TestMethod]
        public void Oracle()
        {
            var act = new TableHelper(SqlType.Oracle).Opimize("table");
            Assert.AreEqual("ALTER TABLE TRUNCATE TABLE \"table\" MOVE;", act);
        }
        [TestMethod]
        public void DB2()
        {
            var act = new TableHelper(SqlType.Db2).Opimize("table");
            Assert.AreEqual("REORG TABLE \"table\";", act);
        }
    }
}
