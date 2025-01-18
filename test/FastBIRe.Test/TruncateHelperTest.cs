namespace FastBIRe.Test
{
    [TestClass]
    public class TruncateHelperTest
    {
        [TestMethod]
        [DataRow(FSqlType.MySql, "DELETE FROM `test`;")]
        [DataRow(FSqlType.SqlServer, "TRUNCATE TABLE [test];")]
        [DataRow(FSqlType.SQLite, "DELETE FROM `test`;")]
        [DataRow(FSqlType.PostgreSql, "TRUNCATE TABLE \"test\";")]
        [DataRow(FSqlType.Oracle, "TRUNCATE TABLE \"test\";")]
        [DataRow(FSqlType.Db2, "TRUNCATE TABLE \"test\";")]
        public void Sql(FSqlType sqlType, string exp)
        {
            var act = new TableHelper(sqlType).Truncate("test");
            Assert.AreEqual(exp, act);
        }
    }
}
