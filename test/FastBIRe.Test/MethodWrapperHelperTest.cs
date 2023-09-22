using Ao.Stock.Querying;

namespace FastBIRe.Test
{
    [TestClass]
    public class MethodWrapperHelperTest
    {
        [TestMethod]
        public void MethodWrapperSqlServer()
        {
            Assert.AreEqual(DefaultMethodWrapper.SqlServer, MethodWrapperExtensions.GetMethodWrapper(SqlType.SqlServer));
            Assert.AreEqual(DefaultMethodWrapper.SqlServer, MethodWrapperExtensions.GetMethodWrapper(SqlType.SqlServerCe));
        }
        [TestMethod]
        public void MethodWrapperMySql()
        {
            Assert.AreEqual(DefaultMethodWrapper.MySql, MethodWrapperExtensions.GetMethodWrapper(SqlType.MySql));
        }
        [TestMethod]
        public void MethodWrapperSqlite()
        {
            Assert.AreEqual(DefaultMethodWrapper.Sqlite, MethodWrapperExtensions.GetMethodWrapper(SqlType.SQLite));
        }
        [TestMethod]
        public void MethodWrapperOracle()
        {
            Assert.AreEqual(DefaultMethodWrapper.Oracle, MethodWrapperExtensions.GetMethodWrapper(SqlType.Oracle));
        }
        [TestMethod]
        public void MethodWrapperPostgreSql()
        {
            Assert.AreEqual(DefaultMethodWrapper.PostgreSql, MethodWrapperExtensions.GetMethodWrapper(SqlType.PostgreSql));
        }
        [TestMethod]
        public void MethodWrapperOther()
        {
            Assert.ThrowsException<NotSupportedException>(()=>MethodWrapperExtensions.GetMethodWrapper(SqlType.Db2));
        }
    }
}
