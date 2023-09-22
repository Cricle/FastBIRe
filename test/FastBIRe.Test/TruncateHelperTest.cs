namespace FastBIRe.Test
{
    [TestClass]
    public class TruncateHelperTest
    {
        [TestMethod]
        [DataRow(SqlType.MySql, "DELETE FROM `test`;")]
        [DataRow(SqlType.SqlServer, "TRUNCATE TABLE [test];")]
        [DataRow(SqlType.SQLite, "DELETE FROM `test`;")]
        [DataRow(SqlType.PostgreSql, "TRUNCATE TABLE \"test\";")]
        [DataRow(SqlType.Oracle, "TRUNCATE TABLE \"test\";")]
        [DataRow(SqlType.Db2, "TRUNCATE TABLE \"test\";")]
        public void Sql(SqlType sqlType,string exp)
        {
            var act = TruncateHelper.Sql("test", sqlType);
            Assert.AreEqual(exp, act);
        }
    }
}
