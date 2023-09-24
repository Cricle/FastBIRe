namespace FastBIRe.Test
{
    [TestClass]
    public class OpimizeHelperTest
    {
        [TestMethod]
        public void MySql()
        {
            var act=OpimizeHelper.Opimize("table", SqlType.MySql);
            Assert.AreEqual("OPTIMIZE TABLE `table`;", act);
        }
        [TestMethod]
        public void SqlServer()
        {
            var act = OpimizeHelper.Opimize("table", SqlType.SqlServer);
            Assert.AreEqual("ALTER INDEX ALL ON [table] REBUILD;", act);
        }
        [TestMethod]
        public void Sqlite()
        {
            var act = OpimizeHelper.Opimize("table", SqlType.SQLite);
            Assert.AreEqual("VACUUM;", act);
        }
        [TestMethod]
        public void PostgreSql()
        {
            var act = OpimizeHelper.Opimize("table", SqlType.PostgreSql);
            Assert.AreEqual("VACUUM FULL \"table\";", act);
        }
        [TestMethod]
        public void Oracle()
        {
            var act = OpimizeHelper.Opimize("table", SqlType.Oracle);
            Assert.AreEqual("ALTER TABLE TRUNCATE TABLE \"table\" MOVE;", act);
        }
        [TestMethod]
        public void DB2()
        {
            var act = OpimizeHelper.Opimize("table", SqlType.Db2);
            Assert.AreEqual("REORG TABLE \"table\";", act);
        }
    }
}
