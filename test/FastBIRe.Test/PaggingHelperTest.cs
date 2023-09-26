namespace FastBIRe.Test
{
    [TestClass]
    public class PaggingHelperTest
    {
        [TestMethod]
        public void Unknow()
        {
            Assert.ThrowsException<NotSupportedException>(() => new TableHelper( SqlType.Db2).Pagging(1, 1));
        }
        [TestMethod]
        public void MySql()
        {
            var act = new TableHelper( SqlType.MySql).Pagging(null, null);
            Assert.AreEqual(string.Empty, act);
            act = new TableHelper(SqlType.MySql).Pagging(11, null);
            Assert.AreEqual("LIMIT 11", act);
            act = new TableHelper(SqlType.MySql).Pagging(null, 11);
            Assert.AreEqual("LIMIT 0, 11", act);
            act = new TableHelper(SqlType.MySql).Pagging(1, 11);
            Assert.AreEqual("LIMIT 1, 11", act);
        }
        [TestMethod]
        public void SqlServer()
        {
            var act = new TableHelper(SqlType.SqlServer).Pagging(null, null);
            Assert.AreEqual(string.Empty, act);
            act = new TableHelper(SqlType.SqlServer).Pagging(11, null);
            Assert.AreEqual("OFFSET 11 ROWS", act);
            act = new TableHelper(SqlType.SqlServer).Pagging(null, 11);
            Assert.AreEqual("OFFSET 0 ROWS FETCH NEXT 11 ROWS ONLY", act);
            act = new TableHelper(SqlType.SqlServer).Pagging(1, 11);
            Assert.AreEqual("OFFSET 1 ROWS FETCH NEXT 11 ROWS ONLY", act);
        }
        [TestMethod]
        public void Sqlite()
        {
            var act = new TableHelper(SqlType.SQLite).Pagging(null, null);
            Assert.AreEqual(string.Empty, act);
            act = new TableHelper(SqlType.SQLite).Pagging(11, null);
            Assert.AreEqual("LIMIT -1 OFFSET 11", act);
            act = new TableHelper(SqlType.SQLite).Pagging(null, 11);
            Assert.AreEqual("LIMIT 11 OFFSET 0", act);
            act = new TableHelper(SqlType.SQLite).Pagging(1, 11);
            Assert.AreEqual("LIMIT 11 OFFSET 1", act);
        }
        [TestMethod]
        public void Postgresql()
        {
            var act = new TableHelper(SqlType.PostgreSql).Pagging(null, null);
            Assert.AreEqual(string.Empty, act);
            act = new TableHelper(SqlType.PostgreSql).Pagging(11, null);
            Assert.AreEqual("OFFSET 11", act);
            act = new TableHelper(SqlType.PostgreSql).Pagging(null, 11);
            Assert.AreEqual("LIMIT 11", act);
            act = new TableHelper(SqlType.PostgreSql).Pagging(1, 11);
            Assert.AreEqual("OFFSET 1 LIMIT 11", act);
        }
    }
}
