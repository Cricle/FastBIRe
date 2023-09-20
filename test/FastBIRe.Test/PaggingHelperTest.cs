namespace FastBIRe.Test
{
    [TestClass]
    public class PaggingHelperTest
    {
        [TestMethod]
        public void Unknow()
        {
            Assert.ThrowsException<NotSupportedException>(() => PaggingHelper.Sql(1, 1, SqlType.Db2));
        }
        [TestMethod]
        public void MySql()
        {
            var act = PaggingHelper.Sql(null, null, SqlType.MySql);
            Assert.AreEqual(string.Empty, act);
            act = PaggingHelper.Sql(11, null, SqlType.MySql);
            Assert.AreEqual("LIMIT 11", act);
            act = PaggingHelper.Sql(null, 11, SqlType.MySql);
            Assert.AreEqual("LIMIT 0, 11", act);
            act = PaggingHelper.Sql(1, 11, SqlType.MySql);
            Assert.AreEqual("LIMIT 1, 11", act);
        }
        [TestMethod]
        public void SqlServer()
        {
            var act = PaggingHelper.Sql(null, null, SqlType.SqlServer);
            Assert.AreEqual(string.Empty, act);
            act = PaggingHelper.Sql(11, null, SqlType.SqlServer);
            Assert.AreEqual("OFFSET 11 ROWS", act);
            act = PaggingHelper.Sql(null, 11, SqlType.SqlServer);
            Assert.AreEqual("OFFSET 0 ROWS FETCH NEXT 11 ROWS ONLY", act);
            act = PaggingHelper.Sql(1, 11, SqlType.SqlServer);
            Assert.AreEqual("OFFSET 1 ROWS FETCH NEXT 11 ROWS ONLY", act);
        }
        [TestMethod]
        public void Sqlite()
        {
            var act = PaggingHelper.Sql(null, null, SqlType.SQLite);
            Assert.AreEqual(string.Empty, act);
            act = PaggingHelper.Sql(11, null, SqlType.SQLite);
            Assert.AreEqual("LIMIT -1 OFFSET 11", act);
            act = PaggingHelper.Sql(null, 11, SqlType.SQLite);
            Assert.AreEqual("LIMIT 11 OFFSET 0", act);
            act = PaggingHelper.Sql(1, 11, SqlType.SQLite);
            Assert.AreEqual("LIMIT 11 OFFSET 1", act);
        }
        [TestMethod]
        public void Postgresql()
        {
            var act = PaggingHelper.Sql(null, null, SqlType.PostgreSql);
            Assert.AreEqual(string.Empty, act);
            act = PaggingHelper.Sql(11, null, SqlType.PostgreSql);
            Assert.AreEqual("OFFSET 11", act);
            act = PaggingHelper.Sql(null, 11, SqlType.PostgreSql);
            Assert.AreEqual("LIMIT 11", act);
            act = PaggingHelper.Sql(1, 11, SqlType.PostgreSql);
            Assert.AreEqual("OFFSET 1 LIMIT 11", act);
        }
    }
}
