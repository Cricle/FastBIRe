using FastBIRe.Wrapping;

namespace FastBIRe.Test
{
    [TestClass]
    public class MethodWrapperHelperTest
    {
        [TestMethod]
        public void MethodWrapperSqlServer()
        {
            Assert.AreEqual(DefaultEscaper.SqlServer, MethodWrapperExtensions.GetMethodWrapper(SqlType.SqlServer));
            Assert.AreEqual(DefaultEscaper.SqlServer, MethodWrapperExtensions.GetMethodWrapper(SqlType.SqlServerCe));
        }
        [TestMethod]
        public void MethodWrapperMySql()
        {
            Assert.AreEqual(DefaultEscaper.MySql, MethodWrapperExtensions.GetMethodWrapper(SqlType.MySql));
        }
        [TestMethod]
        public void MethodWrapperSqlite()
        {
            Assert.AreEqual(DefaultEscaper.Sqlite, MethodWrapperExtensions.GetMethodWrapper(SqlType.SQLite));
        }
        [TestMethod]
        public void MethodWrapperOracle()
        {
            Assert.AreEqual(DefaultEscaper.Oracle, MethodWrapperExtensions.GetMethodWrapper(SqlType.Oracle));
        }
        [TestMethod]
        public void MethodWrapperPostgreSql()
        {
            Assert.AreEqual(DefaultEscaper.PostgreSql, MethodWrapperExtensions.GetMethodWrapper(SqlType.PostgreSql));
        }
        [TestMethod]
        public void MethodWrapperOther()
        {
            Assert.ThrowsException<NotSupportedException>(()=>MethodWrapperExtensions.GetMethodWrapper(SqlType.Db2));
        }
    }
}
