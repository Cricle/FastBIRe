namespace FastBIRe.Test
{
    [TestClass]
    public class ViewHelperTest
    {
        [TestMethod]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        public void Create(SqlType sqlType)
        {
            var wraper = MergeHelper.Wrap(sqlType, "view1");
            var act = ViewHelper.Create("view1", $"SELECT * FROM {wraper}", sqlType);
            Assert.AreEqual($"CREATE VIEW {wraper} AS SELECT * FROM {wraper};", act);
        }
        [TestMethod]
        public void DropSqlServer()
        {
            var act = ViewHelper.Drop("view1", SqlType.SqlServer);
            Assert.AreEqual("IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'view1')) DROP VIEW [view1];", act);
        }
        [TestMethod]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        public void DropMySql(SqlType sqlType)
        {
            var qutoView = MergeHelper.Wrap(sqlType, "view1");
            var act = ViewHelper.Drop("view1", sqlType);
            Assert.AreEqual($"DROP VIEW IF EXISTS {qutoView};", act);
        }
        [TestMethod]
        public void DropIfNoSupport_ReturnEmpty()
        {
            Assert.AreEqual(string.Empty, ViewHelper.Drop("view1", SqlType.Db2));
        }
    }
}
