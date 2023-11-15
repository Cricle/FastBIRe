namespace FastBIRe.Test
{
    [TestClass]
    public class IndexByteLenHelperTest : DbTestBase
    {
        [TestMethod]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.PostgreSql)]
        [DataRow(SqlType.SQLite)]
        public async Task GetIndexByteLenAsync(SqlType sqlType)
        {
            var conn = databaseIniter.Get(sqlType);
            var len = await IndexByteLenHelper.GetIndexByteLenAsync(conn, sqlType);
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    Assert.AreEqual(1, len);
                    break;
                case SqlType.MySql:
                    Assert.AreEqual(768, len);
                    break;
                case SqlType.SQLite:
                    Assert.AreEqual(4096, len);
                    break;
                case SqlType.PostgreSql:
                    Assert.AreEqual(268427264, len);
                    break;
                default:
                    break;
            }
        }
    }
}
