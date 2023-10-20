using FastBIRe.Wrapping;

namespace FastBIRe.Test
{
    [TestClass]
    public class MethodWrapperHelperTest
    {
        [TestMethod]
        public void MethodWrapperSqlServer()
        {
            Assert.AreEqual(DefaultEscaper.SqlServer, MethodWrapperExtensions.GetEscaper(SqlType.SqlServer));
            Assert.AreEqual(DefaultEscaper.SqlServer, MethodWrapperExtensions.GetEscaper(SqlType.SqlServerCe));
        }
        [TestMethod]
        public void MethodWrapperMySql()
        {
            Assert.AreEqual(DefaultEscaper.MySql, MethodWrapperExtensions.GetEscaper(SqlType.MySql));
        }
        [TestMethod]
        public void MethodWrapperSqlite()
        {
            Assert.AreEqual(DefaultEscaper.Sqlite, MethodWrapperExtensions.GetEscaper(SqlType.SQLite));
        }
        [TestMethod]
        public void MethodWrapperOracle()
        {
            Assert.AreEqual(DefaultEscaper.Oracle, MethodWrapperExtensions.GetEscaper(SqlType.Oracle));
        }
        [TestMethod]
        public void MethodWrapperPostgreSql()
        {
            Assert.AreEqual(DefaultEscaper.PostgreSql, MethodWrapperExtensions.GetEscaper(SqlType.PostgreSql));
        }
        [TestMethod]
        public void MethodWrapperOther()
        {
            Assert.ThrowsException<NotSupportedException>(()=>MethodWrapperExtensions.GetEscaper(SqlType.Db2));
        }
    }
}
