using Ao.Stock.Querying;

namespace FastBIRe.Test
{
    [TestClass]
    public class MethodWrapperHelperTest
    {
        [TestMethod]
        public void MethodWrapperSqlServer()
        {
            Assert.AreEqual(DefaultMethodWrapper.SqlServer, MethodWrapperHelper.GetMethodWrapper(SqlType.SqlServer));
            Assert.AreEqual(DefaultMethodWrapper.SqlServer, MethodWrapperHelper.GetMethodWrapper(SqlType.SqlServerCe));
        }
        [TestMethod]
        public void MethodWrapperMySql()
        {
            Assert.AreEqual(DefaultMethodWrapper.MySql, MethodWrapperHelper.GetMethodWrapper(SqlType.MySql));
        }
        [TestMethod]
        public void MethodWrapperSqlite()
        {
            Assert.AreEqual(DefaultMethodWrapper.Sqlite, MethodWrapperHelper.GetMethodWrapper(SqlType.SQLite));
        }
        [TestMethod]
        public void MethodWrapperOracle()
        {
            Assert.AreEqual(DefaultMethodWrapper.Oracle, MethodWrapperHelper.GetMethodWrapper(SqlType.Oracle));
        }
        [TestMethod]
        public void MethodWrapperPostgreSql()
        {
            Assert.AreEqual(DefaultMethodWrapper.PostgreSql, MethodWrapperHelper.GetMethodWrapper(SqlType.PostgreSql));
        }
        [TestMethod]
        public void MethodWrapperOther()
        {
            Assert.ThrowsException<NotSupportedException>(()=>MethodWrapperHelper.GetMethodWrapper(SqlType.Db2));
        }
    }
}
