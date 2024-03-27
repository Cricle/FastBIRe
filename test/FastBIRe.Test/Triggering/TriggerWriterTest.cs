using FastBIRe.Triggering;

namespace FastBIRe.Test.Triggering
{
    [TestClass]
    public class TriggerWriterTest
    {
        [TestMethod]
        public void ThrowIfGivenNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TriggerWriter(null!));
        }
        [TestMethod]
        [DataRow(TriggerTypes.BeforeInsert, SqlType.SqlServer, "BEFORE INSERT")]
        [DataRow(TriggerTypes.BeforeInsert, SqlType.MySql, "BEFORE INSERT")]
        [DataRow(TriggerTypes.BeforeInsert, SqlType.SQLite, "BEFORE INSERT")]
        [DataRow(TriggerTypes.BeforeInsert, SqlType.PostgreSql, "BEFORE INSERT")]
        [DataRow(TriggerTypes.BeforeInsert, SqlType.SqlServerCe, "BEFORE INSERT")]

        [DataRow(TriggerTypes.AfterInsert, SqlType.SqlServer, "AFTER INSERT")]
        [DataRow(TriggerTypes.AfterInsert, SqlType.MySql, "AFTER INSERT")]
        [DataRow(TriggerTypes.AfterInsert, SqlType.SQLite, "AFTER INSERT")]
        [DataRow(TriggerTypes.AfterInsert, SqlType.PostgreSql, "AFTER INSERT")]
        [DataRow(TriggerTypes.AfterInsert, SqlType.SqlServerCe, "AFTER INSERT")]

        [DataRow(TriggerTypes.BeforeUpdate, SqlType.SqlServer, "BEFORE UPDATE")]
        [DataRow(TriggerTypes.BeforeUpdate, SqlType.MySql, "BEFORE UPDATE")]
        [DataRow(TriggerTypes.BeforeUpdate, SqlType.SQLite, "BEFORE UPDATE")]
        [DataRow(TriggerTypes.BeforeUpdate, SqlType.PostgreSql, "BEFORE UPDATE")]
        [DataRow(TriggerTypes.BeforeUpdate, SqlType.SqlServerCe, "BEFORE UPDATE")]

        [DataRow(TriggerTypes.AfterUpdate, SqlType.SqlServer, "AFTER UPDATE")]
        [DataRow(TriggerTypes.AfterUpdate, SqlType.MySql, "AFTER UPDATE")]
        [DataRow(TriggerTypes.AfterUpdate, SqlType.SQLite, "AFTER UPDATE")]
        [DataRow(TriggerTypes.AfterUpdate, SqlType.PostgreSql, "AFTER UPDATE")]
        [DataRow(TriggerTypes.AfterUpdate, SqlType.SqlServerCe, "AFTER UPDATE")]

        [DataRow(TriggerTypes.BeforeMerge, SqlType.SqlServer, "BEFORE MERGE")]
        [DataRow(TriggerTypes.BeforeMerge, SqlType.MySql, "BEFORE MERGE")]
        [DataRow(TriggerTypes.BeforeMerge, SqlType.SQLite, "BEFORE MERGE")]
        [DataRow(TriggerTypes.BeforeMerge, SqlType.PostgreSql, "BEFORE MERGE")]
        [DataRow(TriggerTypes.BeforeMerge, SqlType.SqlServerCe, "BEFORE MERGE")]

        [DataRow(TriggerTypes.AfterMerge, SqlType.SqlServer, "AFTER MERGE")]
        [DataRow(TriggerTypes.AfterMerge, SqlType.MySql, "AFTER MERGE")]
        [DataRow(TriggerTypes.AfterMerge, SqlType.SQLite, "AFTER MERGE")]
        [DataRow(TriggerTypes.AfterMerge, SqlType.PostgreSql, "AFTER MERGE")]
        [DataRow(TriggerTypes.AfterMerge, SqlType.SqlServerCe, "AFTER MERGE")]

        [DataRow(TriggerTypes.BeforeDelete, SqlType.SqlServer, "BEFORE DELETE")]
        [DataRow(TriggerTypes.BeforeDelete, SqlType.MySql, "BEFORE DELETE")]
        [DataRow(TriggerTypes.BeforeDelete, SqlType.SQLite, "BEFORE DELETE")]
        [DataRow(TriggerTypes.BeforeDelete, SqlType.PostgreSql, "BEFORE DELETE")]
        [DataRow(TriggerTypes.BeforeDelete, SqlType.SqlServerCe, "BEFORE DELETE")]

        [DataRow(TriggerTypes.AfterDelete, SqlType.SqlServer, "AFTER DELETE")]
        [DataRow(TriggerTypes.AfterDelete, SqlType.MySql, "AFTER DELETE")]
        [DataRow(TriggerTypes.AfterDelete, SqlType.SQLite, "AFTER DELETE")]
        [DataRow(TriggerTypes.AfterDelete, SqlType.PostgreSql, "AFTER DELETE")]
        [DataRow(TriggerTypes.AfterDelete, SqlType.SqlServerCe, "AFTER DELETE")]

        [DataRow(TriggerTypes.InsteadOfInsert, SqlType.SqlServer, "INSTEAD OF INSERT")]
        [DataRow(TriggerTypes.InsteadOfUpdate, SqlType.SqlServer, "INSTEAD OF UPDATE")]
        [DataRow(TriggerTypes.InsteadOfDelete, SqlType.SqlServer, "INSTEAD OF DELETE")]
        public void TriggerName(TriggerTypes triggerTypes, SqlType sqlType, string act)
        {
            Assert.AreEqual(act, TriggerWriter.Default.GetTriggerName(triggerTypes, sqlType));
        }
        [TestMethod]
        public void TriggerName_Empty()
        {
            Assert.AreEqual(string.Empty, TriggerWriter.Default.GetTriggerName(TriggerTypes.None, SqlType.MySql));
        }
        [TestMethod]
        [DataRow(SqlType.MySql, "test", new[] { "DROP TRIGGER IF EXISTS `test`;" })]
        [DataRow(SqlType.SqlServer, "test", new[] { "DROP TRIGGER IF EXISTS [test];" })]
        [DataRow(SqlType.SqlServerCe, "test", new[] { "DROP TRIGGER IF EXISTS [test];" })]
        [DataRow(SqlType.SQLite, "test", new[] { "DROP TRIGGER IF EXISTS `test`;" })]
        [DataRow(SqlType.PostgreSql, "test", new[] { @"DO $$ 
BEGIN 
  IF EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'test') THEN 
    DROP TRIGGER ""test"" ON ""test""; 
  END IF;
END $$;", "DROP FUNCTION IF EXISTS \"fun_test\";" })]
        [DataRow(SqlType.Db2, "test", new string[] { })]
        [DataRow(SqlType.Oracle, "test", new string[] { })]
        public void Drop(SqlType sqlType, string name, string[] results)
        {
            var res = TriggerWriter.Default.Drop(sqlType, name, "test").ToList();
            Assert.AreEqual(results.Length, res.Count);
            for (int i = 0; i < res.Count; i++)
            {
                Assert.AreEqual(results[i], res[i]);
            }
        }
        [TestMethod]
        [DataRow(SqlType.MySql, "trigger", TriggerTypes.BeforeInsert, "table", "SELECT 1;", null, new[] { $@"
CREATE TRIGGER `trigger` BEFORE INSERT ON `table`
FOR EACH ROW
BEGIN
SELECT 1;
END;
" })]
        [DataRow(SqlType.MySql, "trigger", TriggerTypes.AfterInsert, "table", "SELECT 1;", "1", new[] { $@"
CREATE TRIGGER `trigger` AFTER INSERT ON `table`
FOR EACH ROW
BEGIN
IF 1 THEN
SELECT 1;
END IF;
END;
" })]
        [DataRow(SqlType.SqlServer, "trigger", TriggerTypes.BeforeInsert, "table", "SELECT 1;", null, new[] { $@"
CREATE TRIGGER [trigger] ON [table] BEFORE INSERT
AS
BEGIN
SELECT 1;
END;
" })]
        [DataRow(SqlType.SqlServer, "trigger", TriggerTypes.AfterInsert, "table", "SELECT 1;", "1", new[] { $@"
CREATE TRIGGER [trigger] ON [table] AFTER INSERT
AS
BEGIN
IF (1)
BEGIN
SELECT 1;
END
END;
" })]
        [DataRow(SqlType.SQLite, "trigger", TriggerTypes.BeforeInsert, "table", "SELECT 1;", null, new[] { $@"
CREATE TRIGGER `trigger` BEFORE INSERT ON `table`

BEGIN
SELECT 1;
END;
" })]
        [DataRow(SqlType.SQLite, "trigger", TriggerTypes.AfterInsert, "table", "SELECT 1;", "1", new[] { $@"
CREATE TRIGGER `trigger` AFTER INSERT ON `table`
WHEN 1
BEGIN
SELECT 1;
END;
" })]
        [DataRow(SqlType.PostgreSql, "trigger", TriggerTypes.BeforeInsert, "table", "SELECT 1;", null, new[] { $@"
CREATE OR REPLACE FUNCTION fun_trigger() RETURNS TRIGGER AS $$
BEGIN
SELECT 1;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER ""trigger"" BEFORE INSERT ON ""table""
FOR EACH ROW
EXECUTE FUNCTION fun_trigger();
" })]
        [DataRow(SqlType.PostgreSql, "trigger", TriggerTypes.AfterInsert, "table", "SELECT 1;", "1", new[] { $@"
CREATE OR REPLACE FUNCTION fun_trigger() RETURNS TRIGGER AS $$
BEGIN
IF 1 THEN
SELECT 1;
END IF;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER ""trigger"" AFTER INSERT ON ""table""
FOR EACH ROW
EXECUTE FUNCTION fun_trigger();
" })]
        public void Create(SqlType sqlType, string name, TriggerTypes type, string table, string body, string? when, string[] results)
        {
            var res = TriggerWriter.Default.Create(sqlType, name, type, table, body, when).ToList();
            Assert.AreEqual(results.Length, res.Count);
            for (int i = 0; i < res.Count; i++)
            {
                Assert.AreEqual(results[i], res[i]);
            }
        }
        [TestMethod]
        [DataRow(SqlType.Db2)]
        [DataRow(SqlType.Oracle)]
        public void CreateEmpty(SqlType sqlType)
        {
            Assert.AreEqual(0, TriggerWriter.Default.Create(sqlType, "name", TriggerTypes.AfterDelete, "tb", string.Empty, null).Count());
        }
    }
}
