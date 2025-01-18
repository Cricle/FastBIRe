namespace FastBIRe.Test
{
    [TestClass]
    public class OpimizeHelperTest
    {
        [TestMethod]
        public void MySql()
        {
            var act = new TableHelper(FSqlType.MySql).Opimize("table");
            Assert.AreEqual("OPTIMIZE TABLE `table`;", act);
        }
        [TestMethod]
        public void SqlServer()
        {
            var act = new TableHelper(FSqlType.SqlServer).Opimize("table");
            Assert.AreEqual("ALTER INDEX ALL ON [table] REBUILD;", act);
        }
        [TestMethod]
        public void Sqlite()
        {
            var act = new TableHelper(FSqlType.SQLite).Opimize("table");
            Assert.AreEqual("VACUUM;", act);
        }
        [TestMethod]
        public void PostgreSql()
        {
            var act = new TableHelper(FSqlType.PostgreSql).Opimize("table");
            Assert.AreEqual("VACUUM FULL \"table\";", act);
        }
        [TestMethod]
        public void Oracle()
        {
            var act = new TableHelper(FSqlType.Oracle).Opimize("table");
            Assert.AreEqual("ALTER TABLE TRUNCATE TABLE \"table\" MOVE;", act);
        }
        [TestMethod]
        public void DB2()
        {
            var act = new TableHelper(FSqlType.Db2).Opimize("table");
            Assert.AreEqual("REORG TABLE \"table\";", act);
        }
    }
}
