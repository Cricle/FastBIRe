namespace FastBIRe.Test
{
    [TestClass]
    public class IndexByteLenHelperTest : DbTestBase
    {
        [TestMethod]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.PostgreSql)]
        [DataRow(FSqlType.SQLite)]
        public async Task GetIndexByteLenAsync(FSqlType sqlType)
        {
            var conn = databaseIniter.Get(sqlType);
            var len = await IndexByteLenHelper.GetIndexByteLenAsync(conn, sqlType);
            switch (sqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    Assert.AreEqual(1, len);
                    break;
                case FSqlType.MySql:
                    Assert.AreEqual(768, len);
                    break;
                case FSqlType.SQLite:
                    Assert.AreEqual(4096, len);
                    break;
                case FSqlType.PostgreSql:
                    Assert.AreEqual(268427264, len);
                    break;
                default:
                    break;
            }
        }
    }
}
