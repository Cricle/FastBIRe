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
        [DataRow(TriggerTypes.BeforeInsert, FSqlType.SqlServer, "BEFORE INSERT")]
        [DataRow(TriggerTypes.BeforeInsert, FSqlType.MySql, "BEFORE INSERT")]
        [DataRow(TriggerTypes.BeforeInsert, FSqlType.SQLite, "BEFORE INSERT")]
        [DataRow(TriggerTypes.BeforeInsert, FSqlType.PostgreSql, "BEFORE INSERT")]
        [DataRow(TriggerTypes.BeforeInsert, FSqlType.SqlServerCe, "BEFORE INSERT")]

        [DataRow(TriggerTypes.AfterInsert, FSqlType.SqlServer, "AFTER INSERT")]
        [DataRow(TriggerTypes.AfterInsert, FSqlType.MySql, "AFTER INSERT")]
        [DataRow(TriggerTypes.AfterInsert, FSqlType.SQLite, "AFTER INSERT")]
        [DataRow(TriggerTypes.AfterInsert, FSqlType.PostgreSql, "AFTER INSERT")]
        [DataRow(TriggerTypes.AfterInsert, FSqlType.SqlServerCe, "AFTER INSERT")]

        [DataRow(TriggerTypes.BeforeUpdate, FSqlType.SqlServer, "BEFORE UPDATE")]
        [DataRow(TriggerTypes.BeforeUpdate, FSqlType.MySql, "BEFORE UPDATE")]
        [DataRow(TriggerTypes.BeforeUpdate, FSqlType.SQLite, "BEFORE UPDATE")]
        [DataRow(TriggerTypes.BeforeUpdate, FSqlType.PostgreSql, "BEFORE UPDATE")]
        [DataRow(TriggerTypes.BeforeUpdate, FSqlType.SqlServerCe, "BEFORE UPDATE")]

        [DataRow(TriggerTypes.AfterUpdate, FSqlType.SqlServer, "AFTER UPDATE")]
        [DataRow(TriggerTypes.AfterUpdate, FSqlType.MySql, "AFTER UPDATE")]
        [DataRow(TriggerTypes.AfterUpdate, FSqlType.SQLite, "AFTER UPDATE")]
        [DataRow(TriggerTypes.AfterUpdate, FSqlType.PostgreSql, "AFTER UPDATE")]
        [DataRow(TriggerTypes.AfterUpdate, FSqlType.SqlServerCe, "AFTER UPDATE")]

        [DataRow(TriggerTypes.BeforeMerge, FSqlType.SqlServer, "BEFORE MERGE")]
        [DataRow(TriggerTypes.BeforeMerge, FSqlType.MySql, "BEFORE MERGE")]
        [DataRow(TriggerTypes.BeforeMerge, FSqlType.SQLite, "BEFORE MERGE")]
        [DataRow(TriggerTypes.BeforeMerge, FSqlType.PostgreSql, "BEFORE MERGE")]
        [DataRow(TriggerTypes.BeforeMerge, FSqlType.SqlServerCe, "BEFORE MERGE")]

        [DataRow(TriggerTypes.AfterMerge, FSqlType.SqlServer, "AFTER MERGE")]
        [DataRow(TriggerTypes.AfterMerge, FSqlType.MySql, "AFTER MERGE")]
        [DataRow(TriggerTypes.AfterMerge, FSqlType.SQLite, "AFTER MERGE")]
        [DataRow(TriggerTypes.AfterMerge, FSqlType.PostgreSql, "AFTER MERGE")]
        [DataRow(TriggerTypes.AfterMerge, FSqlType.SqlServerCe, "AFTER MERGE")]

        [DataRow(TriggerTypes.BeforeDelete, FSqlType.SqlServer, "BEFORE DELETE")]
        [DataRow(TriggerTypes.BeforeDelete, FSqlType.MySql, "BEFORE DELETE")]
        [DataRow(TriggerTypes.BeforeDelete, FSqlType.SQLite, "BEFORE DELETE")]
        [DataRow(TriggerTypes.BeforeDelete, FSqlType.PostgreSql, "BEFORE DELETE")]
        [DataRow(TriggerTypes.BeforeDelete, FSqlType.SqlServerCe, "BEFORE DELETE")]

        [DataRow(TriggerTypes.AfterDelete, FSqlType.SqlServer, "AFTER DELETE")]
        [DataRow(TriggerTypes.AfterDelete, FSqlType.MySql, "AFTER DELETE")]
        [DataRow(TriggerTypes.AfterDelete, FSqlType.SQLite, "AFTER DELETE")]
        [DataRow(TriggerTypes.AfterDelete, FSqlType.PostgreSql, "AFTER DELETE")]
        [DataRow(TriggerTypes.AfterDelete, FSqlType.SqlServerCe, "AFTER DELETE")]

        [DataRow(TriggerTypes.InsteadOfInsert, FSqlType.SqlServer, "INSTEAD OF INSERT")]
        [DataRow(TriggerTypes.InsteadOfUpdate, FSqlType.SqlServer, "INSTEAD OF UPDATE")]
        [DataRow(TriggerTypes.InsteadOfDelete, FSqlType.SqlServer, "INSTEAD OF DELETE")]
        public void TriggerName(TriggerTypes triggerTypes, FSqlType sqlType, string act)
        {
            Assert.AreEqual(act, TriggerWriter.Default.GetTriggerName(triggerTypes, sqlType));
        }
        [TestMethod]
        public void TriggerName_Empty()
        {
            Assert.AreEqual(string.Empty, TriggerWriter.Default.GetTriggerName(TriggerTypes.None, FSqlType.MySql));
        }
        [TestMethod]
        [DataRow(FSqlType.MySql, "test", new[] { "DROP TRIGGER IF EXISTS `test`;" })]
        [DataRow(FSqlType.SqlServer, "test", new[] { "DROP TRIGGER IF EXISTS [test];" })]
        [DataRow(FSqlType.SqlServerCe, "test", new[] { "DROP TRIGGER IF EXISTS [test];" })]
        [DataRow(FSqlType.SQLite, "test", new[] { "DROP TRIGGER IF EXISTS `test`;" })]
        [DataRow(FSqlType.PostgreSql, "test", new[] { @"DO $$ 
BEGIN 
  IF EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'test') THEN 
    DROP TRIGGER ""test"" ON ""test""; 
  END IF;
END $$;", "DROP FUNCTION IF EXISTS \"fun_test\";" })]
        [DataRow(FSqlType.Db2, "test", new string[] { })]
        [DataRow(FSqlType.Oracle, "test", new string[] { })]
        public void Drop(FSqlType sqlType, string name, string[] results)
        {
            var res = TriggerWriter.Default.Drop(sqlType, name, "test").ToList();
            Assert.AreEqual(results.Length, res.Count);
            for (int i = 0; i < res.Count; i++)
            {
                Assert.AreEqual(results[i], res[i]);
            }
        }
        [TestMethod]
        [DataRow(FSqlType.MySql, "trigger", TriggerTypes.BeforeInsert, "table", "SELECT 1;", null, new[] { $@"
CREATE TRIGGER `trigger` BEFORE INSERT ON `table`
FOR EACH ROW
BEGIN
SELECT 1;
END;
" })]
        [DataRow(FSqlType.MySql, "trigger", TriggerTypes.AfterInsert, "table", "SELECT 1;", "1", new[] { $@"
CREATE TRIGGER `trigger` AFTER INSERT ON `table`
FOR EACH ROW
BEGIN
IF 1 THEN
SELECT 1;
END IF;
END;
" })]
        [DataRow(FSqlType.SqlServer, "trigger", TriggerTypes.BeforeInsert, "table", "SELECT 1;", null, new[] { $@"
CREATE TRIGGER [trigger] ON [table] BEFORE INSERT
AS
BEGIN
SELECT 1;
END;
" })]
        [DataRow(FSqlType.SqlServer, "trigger", TriggerTypes.AfterInsert, "table", "SELECT 1;", "1", new[] { $@"
CREATE TRIGGER [trigger] ON [table] AFTER INSERT
AS
BEGIN
IF (1)
BEGIN
SELECT 1;
END
END;
" })]
        [DataRow(FSqlType.SQLite, "trigger", TriggerTypes.BeforeInsert, "table", "SELECT 1;", null, new[] { $@"
CREATE TRIGGER `trigger` BEFORE INSERT ON `table`

BEGIN
SELECT 1;
END;
" })]
        [DataRow(FSqlType.SQLite, "trigger", TriggerTypes.AfterInsert, "table", "SELECT 1;", "1", new[] { $@"
CREATE TRIGGER `trigger` AFTER INSERT ON `table`
WHEN 1
BEGIN
SELECT 1;
END;
" })]
        [DataRow(FSqlType.PostgreSql, "trigger", TriggerTypes.BeforeInsert, "table", "SELECT 1;", null, new[] { $@"
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
        [DataRow(FSqlType.PostgreSql, "trigger", TriggerTypes.AfterInsert, "table", "SELECT 1;", "1", new[] { $@"
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
        public void Create(FSqlType sqlType, string name, TriggerTypes type, string table, string body, string? when, string[] results)
        {
            var res = TriggerWriter.Default.Create(sqlType, name, type, table, body, when).ToList();
            Assert.AreEqual(results.Length, res.Count);
            for (int i = 0; i < res.Count; i++)
            {
                Assert.AreEqual(results[i], res[i]);
            }
        }
        [TestMethod]
        [DataRow(FSqlType.Db2)]
        [DataRow(FSqlType.Oracle)]
        public void CreateEmpty(FSqlType sqlType)
        {
            Assert.AreEqual(0, TriggerWriter.Default.Create(sqlType, "name", TriggerTypes.AfterDelete, "tb", string.Empty, null).Count());
        }
    }
}
