using FastBIRe.Wrapping;

namespace FastBIRe.Test
{
    [TestClass]
    public class SqlTypGetExtensionsTest
    {
        [TestMethod]
        public void MethodWrapperSqlServer()
        {
            Assert.AreEqual(DefaultEscaper.SqlServer, SqlTypGetExtensions.GetEscaper(FSqlType.SqlServer));
            Assert.AreEqual(DefaultEscaper.SqlServer, SqlTypGetExtensions.GetEscaper(FSqlType.SqlServerCe));
        }
        [TestMethod]
        public void MethodWrapperMySql()
        {
            Assert.AreEqual(DefaultEscaper.MySql, SqlTypGetExtensions.GetEscaper(FSqlType.MySql));
        }
        [TestMethod]
        public void MethodWrapperSqlite()
        {
            Assert.AreEqual(DefaultEscaper.Sqlite, SqlTypGetExtensions.GetEscaper(FSqlType.SQLite));
        }
        [TestMethod]
        public void MethodWrapperOracle()
        {
            Assert.AreEqual(DefaultEscaper.Oracle, SqlTypGetExtensions.GetEscaper(FSqlType.Oracle));
        }
        [TestMethod]
        public void MethodWrapperPostgreSql()
        {
            Assert.AreEqual(DefaultEscaper.PostgreSql, SqlTypGetExtensions.GetEscaper(FSqlType.PostgreSql));
        }
        [TestMethod]
        public void MethodWrapperOther()
        {
            Assert.ThrowsException<NotSupportedException>(() => SqlTypGetExtensions.GetEscaper(FSqlType.Db2));
        }
    }
}
