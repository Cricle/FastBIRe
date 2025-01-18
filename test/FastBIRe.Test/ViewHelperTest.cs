namespace FastBIRe.Test
{
    [TestClass]
    public class ViewHelperTest
    {
        [TestMethod]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        public void Create(FSqlType sqlType)
        {
            var wraper = sqlType.Wrap("view1");
            var act = new TableHelper(sqlType).CreateView("view1", $"SELECT * FROM {wraper}");
            Assert.AreEqual($"CREATE VIEW {wraper} AS SELECT * FROM {wraper};", act);
        }
        [TestMethod]
        public void DropSqlServer()
        {
            var act = new TableHelper(FSqlType.SqlServer).DropView("view1");
            Assert.AreEqual("IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'view1')) DROP VIEW [view1];", act);
        }
        [TestMethod]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        public void DropMySql(FSqlType sqlType)
        {
            var qutoView = sqlType.Wrap("view1");
            var act = new TableHelper(sqlType).DropView("view1");
            Assert.AreEqual($"DROP VIEW IF EXISTS {qutoView};", act);
        }
        [TestMethod]
        public void DropIfNoSupport_ReturnEmpty()
        {
            Assert.AreEqual(string.Empty, new TableHelper(FSqlType.Oracle).DropView("view1"));
        }
    }
}
