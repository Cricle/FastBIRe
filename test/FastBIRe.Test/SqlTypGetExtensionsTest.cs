using FastBIRe.Wrapping;

namespace FastBIRe.Test
{
    [TestClass]
    public class SqlTypGetExtensionsTest
    {
        [TestMethod]
        public void MethodWrapperSqlServer()
        {
            Assert.AreEqual(DefaultEscaper.SqlServer, SqlTypGetExtensions.GetEscaper(SqlType.SqlServer));
            Assert.AreEqual(DefaultEscaper.SqlServer, SqlTypGetExtensions.GetEscaper(SqlType.SqlServerCe));
        }
        [TestMethod]
        public void MethodWrapperMySql()
        {
            Assert.AreEqual(DefaultEscaper.MySql, SqlTypGetExtensions.GetEscaper(SqlType.MySql));
        }
        [TestMethod]
        public void MethodWrapperSqlite()
        {
            Assert.AreEqual(DefaultEscaper.Sqlite, SqlTypGetExtensions.GetEscaper(SqlType.SQLite));
        }
        [TestMethod]
        public void MethodWrapperOracle()
        {
            Assert.AreEqual(DefaultEscaper.Oracle, SqlTypGetExtensions.GetEscaper(SqlType.Oracle));
        }
        [TestMethod]
        public void MethodWrapperPostgreSql()
        {
            Assert.AreEqual(DefaultEscaper.PostgreSql, SqlTypGetExtensions.GetEscaper(SqlType.PostgreSql));
        }
        [TestMethod]
        public void MethodWrapperOther()
        {
            Assert.ThrowsException<NotSupportedException>(()=>SqlTypGetExtensions.GetEscaper(SqlType.Db2));
        }
    }
}
